using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Commands.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator(IPasswordPolicy passwordPolicy)
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .Must(password => passwordPolicy.Validate(password).Count == 0)
            .WithMessage("Password does not meet the required policy.");
    }
}

public sealed class ChangePasswordCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    ICurrentUserContext currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditLogger auditLogger)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), currentUser.UserId ?? Guid.Empty);

        if (!passwordHasher.VerifyPassword(user.PasswordHash, request.CurrentPassword))
        {
            throw new AuthenticationFailedException("Current password is incorrect.");
        }

        var now = dateTimeProvider.UtcNow;

        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        user.MustChangePassword = false;

        // HIGH finding, docs/ARCHITECTURE_REVIEW.md: unlike ResetPasswordCommand,
        // this handler left every other active session's refresh token valid
        // after a password change, undermining "change my password" as an
        // incident-response action. Revoke them here too, for consistency.
        var activeRefreshTokens = await db.RefreshTokens
            .IgnoreQueryFilters()
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var token in activeRefreshTokens)
        {
            token.RevokedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("auth.password_changed", nameof(User), user.Id.ToString(), cancellationToken: cancellationToken);
    }
}
