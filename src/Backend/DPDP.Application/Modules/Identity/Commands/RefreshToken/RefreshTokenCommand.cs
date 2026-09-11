using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Identity.DTOs;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DPDP.Application.Modules.Identity.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResultDto>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public sealed class RefreshTokenCommandHandler(
    IAppDbContext db,
    IJwtTokenService jwtTokenService,
    ISecureTokenGenerator secureTokenGenerator,
    IDateTimeProvider dateTimeProvider,
    IRequestContext requestContext,
    IAuditLogger auditLogger,
    IOptions<AccountSecurityOptions> accountSecurityOptions)
    : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly AccountSecurityOptions _options = accountSecurityOptions.Value;

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var tokenHash = secureTokenGenerator.Hash(request.RefreshToken);

        // Pre-authentication flow — see the same note in LoginCommand.
        var existingToken = await db.RefreshTokens.IgnoreQueryFilters()
            .Include(rt => rt.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null)
        {
            throw new AuthenticationFailedException("Invalid refresh token.");
        }

        if (existingToken.RevokedAt is not null)
        {
            // Reuse of an already-rotated/revoked token: treat as a
            // compromise signal and revoke every active token for this
            // user, forcing re-authentication everywhere.
            var allActive = await db.RefreshTokens.IgnoreQueryFilters()
                .Where(rt => rt.UserId == existingToken.UserId && rt.RevokedAt == null)
                .ToListAsync(cancellationToken);
            foreach (var token in allActive)
            {
                token.RevokedAt = now;
            }

            await db.SaveChangesAsync(cancellationToken);
            await auditLogger.LogAsync("auth.refresh_token_reuse_detected", nameof(User), existingToken.UserId.ToString(), cancellationToken: cancellationToken);
            throw new AuthenticationFailedException("Invalid refresh token.");
        }

        if (existingToken.ExpiresAt <= now)
        {
            throw new AuthenticationFailedException("Invalid refresh token.");
        }

        var user = existingToken.User;
        if (!user.IsActive || user.IsDeleted)
        {
            throw new AccountDisabledException();
        }

        var rawRefreshToken = secureTokenGenerator.GenerateToken();
        var newToken = new DPDP.Domain.Modules.Identity.RefreshToken
        {
            UserId = user.Id,
            TokenHash = secureTokenGenerator.Hash(rawRefreshToken),
            ExpiresAt = now.AddDays(_options.RefreshTokenDays),
            CreatedAt = now,
            CreatedByIp = requestContext.IpAddress,
        };
        db.RefreshTokens.Add(newToken);

        existingToken.RevokedAt = now;
        existingToken.ReplacedByTokenId = newToken.Id;

        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
        var isSuperAdministrator = roleNames.Contains(RoleNames.SuperAdministrator);
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Key)
            .Distinct()
            .ToList();

        var accessToken = jwtTokenService.GenerateAccessToken(user, roleNames, permissions, isSuperAdministrator);

        await db.SaveChangesAsync(cancellationToken);

        var me = new MeDto(user.Id, user.OrganisationId, user.Email, user.FullName, isSuperAdministrator, roleNames, permissions);

        return new AuthResultDto(accessToken.Token, accessToken.ExpiresAt, rawRefreshToken, newToken.ExpiresAt, me);
    }
}
