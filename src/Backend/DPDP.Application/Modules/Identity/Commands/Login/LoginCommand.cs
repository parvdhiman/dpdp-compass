using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Identity.DTOs;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DPDP.Application.Modules.Identity.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResultDto>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    ISecureTokenGenerator secureTokenGenerator,
    IDateTimeProvider dateTimeProvider,
    IRequestContext requestContext,
    IAuditLogger auditLogger,
    IOptions<AccountSecurityOptions> accountSecurityOptions)
    : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly AccountSecurityOptions _options = accountSecurityOptions.Value;

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        // No tenant context exists yet at login — IgnoreQueryFilters is
        // required here, not a bypass of tenant isolation. See
        // docs/ARCHITECTURE.md section 4 and the Module 2 completion report.
        var user = await db.Users.IgnoreQueryFilters()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            await RecordLoginHistoryAsync(null, null, request.Email, false, "no_such_account", now, cancellationToken);
            throw new AuthenticationFailedException("Invalid email or password.");
        }

        if (user.LockoutEnd is { } lockoutEnd && lockoutEnd > now)
        {
            await RecordLoginHistoryAsync(user.Id, user.OrganisationId, request.Email, false, "locked_out", now, cancellationToken);
            throw new AccountLockedException(lockoutEnd);
        }

        if (!user.IsActive)
        {
            await RecordLoginHistoryAsync(user.Id, user.OrganisationId, request.Email, false, "disabled", now, cancellationToken);
            throw new AccountDisabledException();
        }

        if (!passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            user.FailedLoginCount += 1;
            if (user.FailedLoginCount >= _options.MaxFailedLoginAttempts)
            {
                user.LockoutEnd = now.AddMinutes(_options.LockoutMinutes);
                user.FailedLoginCount = 0;
            }

            await db.SaveChangesAsync(cancellationToken);
            await RecordLoginHistoryAsync(user.Id, user.OrganisationId, request.Email, false, "bad_password", now, cancellationToken);
            throw new AuthenticationFailedException("Invalid email or password.");
        }

        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = now;

        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
        var isSuperAdministrator = roleNames.Contains(RoleNames.SuperAdministrator);
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Key)
            .Distinct()
            .ToList();

        var accessToken = jwtTokenService.GenerateAccessToken(user, roleNames, permissions, isSuperAdministrator);

        var rawRefreshToken = secureTokenGenerator.GenerateToken();
        var refreshToken = new DPDP.Domain.Modules.Identity.RefreshToken
        {
            UserId = user.Id,
            TokenHash = secureTokenGenerator.Hash(rawRefreshToken),
            ExpiresAt = now.AddDays(_options.RefreshTokenDays),
            CreatedAt = now,
            CreatedByIp = requestContext.IpAddress,
        };
        db.RefreshTokens.Add(refreshToken);

        await db.SaveChangesAsync(cancellationToken);
        await RecordLoginHistoryAsync(user.Id, user.OrganisationId, request.Email, true, null, now, cancellationToken);
        await auditLogger.LogAsync("auth.login", nameof(User), user.Id.ToString(), cancellationToken: cancellationToken);

        var me = new MeDto(user.Id, user.OrganisationId, user.Email, user.FullName, isSuperAdministrator, roleNames, permissions);

        return new AuthResultDto(
            accessToken.Token,
            accessToken.ExpiresAt,
            rawRefreshToken,
            refreshToken.ExpiresAt,
            me);
    }

    private async Task RecordLoginHistoryAsync(
        Guid? userId,
        Guid? organisationId,
        string attemptedEmail,
        bool succeeded,
        string? failureReason,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        db.LoginHistories.Add(new LoginHistory
        {
            UserId = userId,
            OrganisationId = organisationId,
            AttemptedEmail = attemptedEmail,
            Succeeded = succeeded,
            FailureReason = failureReason,
            IpAddress = requestContext.IpAddress,
            UserAgent = requestContext.UserAgent,
            CreatedAt = now,
        });

        await db.SaveChangesAsync(cancellationToken);

        // HIGH finding, docs/ARCHITECTURE_REVIEW.md: failed/locked-out/disabled
        // login attempts were recorded only in LoginHistory, never in the
        // unified audit_logs sink MASTER_PROMPT §8 explicitly requires
        // "failed login" to be tracked in — a future GET /audit-logs reader
        // would silently miss every one of these events. Successful logins
        // are still logged once, separately, via the "auth.login" call below.
        if (!succeeded)
        {
            await auditLogger.LogAsync(
                "auth.login_failed",
                nameof(User),
                userId?.ToString(),
                newValue: new { attemptedEmail, failureReason },
                cancellationToken: cancellationToken);
        }
    }
}
