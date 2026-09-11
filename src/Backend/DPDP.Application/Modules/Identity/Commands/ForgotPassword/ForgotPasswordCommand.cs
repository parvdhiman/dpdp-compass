using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DPDP.Application.Modules.Identity.Commands.ForgotPassword;

/// <summary>
/// ResetToken is always populated by the handler when an account exists —
/// it is the Api layer's job (not Application's) to decide whether to ever
/// put it on the wire, and only in Development, since there is no email
/// delivery module yet (Notifications is Phase 7). See the Module 2
/// completion report's Known Issues.
/// </summary>
public sealed record ForgotPasswordResult(string? ResetToken);

public sealed record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResult>;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public sealed class ForgotPasswordCommandHandler(
    IAppDbContext db,
    ISecureTokenGenerator secureTokenGenerator,
    IDateTimeProvider dateTimeProvider,
    IAuditLogger auditLogger,
    IOptions<AccountSecurityOptions> accountSecurityOptions)
    : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private readonly AccountSecurityOptions _options = accountSecurityOptions.Value;

    public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        // Pre-authentication flow — see the note in LoginCommand. Always
        // returns the same shape regardless of whether the account exists,
        // to avoid user enumeration via response timing/shape.
        var user = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && !u.IsDeleted && u.IsActive, cancellationToken);

        if (user is null)
        {
            return new ForgotPasswordResult(null);
        }

        var now = dateTimeProvider.UtcNow;
        var rawToken = secureTokenGenerator.GenerateToken();

        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = secureTokenGenerator.Hash(rawToken),
            ExpiresAt = now.AddMinutes(_options.PasswordResetTokenMinutes),
            CreatedAt = now,
        });

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("auth.password_reset_requested", nameof(User), user.Id.ToString(), cancellationToken: cancellationToken);

        return new ForgotPasswordResult(rawToken);
    }
}
