using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Token, string NewPassword) : IRequest;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator(IPasswordPolicy passwordPolicy)
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .Must(password => passwordPolicy.Validate(password).Count == 0)
            .WithMessage("Password does not meet the required policy.");
    }
}

public sealed class ResetPasswordCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    ISecureTokenGenerator secureTokenGenerator,
    IDateTimeProvider dateTimeProvider,
    IAuditLogger auditLogger)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = secureTokenGenerator.Hash(request.Token);
        var now = dateTimeProvider.UtcNow;

        // Pre-authentication flow — see the note in LoginCommand.
        var resetToken = await db.PasswordResetTokens.IgnoreQueryFilters()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (resetToken is null || resetToken.UsedAt is not null || resetToken.ExpiresAt <= now)
        {
            throw new AuthenticationFailedException("Invalid or expired reset token.");
        }

        var user = resetToken.User;
        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        user.MustChangePassword = false;
        user.FailedLoginCount = 0;
        user.LockoutEnd = null;

        resetToken.UsedAt = now;

        // A successful reset invalidates every existing session.
        var activeRefreshTokens = await db.RefreshTokens
            .IgnoreQueryFilters()
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var token in activeRefreshTokens)
        {
            token.RevokedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("auth.password_reset_completed", nameof(User), user.Id.ToString(), cancellationToken: cancellationToken);
    }
}
