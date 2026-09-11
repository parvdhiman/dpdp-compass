using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

/// <summary>
/// Revokes exactly the presented refresh token, and only if it belongs to
/// the caller — a user can never revoke another user's session this way.
/// </summary>
public sealed class LogoutCommandHandler(
    IAppDbContext db,
    ISecureTokenGenerator secureTokenGenerator,
    ICurrentUserContext currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditLogger auditLogger)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = secureTokenGenerator.Hash(request.RefreshToken);

        var token = await db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.UserId == currentUser.UserId, cancellationToken);

        if (token is null || token.RevokedAt is not null)
        {
            return;
        }

        token.RevokedAt = dateTimeProvider.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("auth.logout", nameof(User), currentUser.UserId?.ToString(), cancellationToken: cancellationToken);
    }
}
