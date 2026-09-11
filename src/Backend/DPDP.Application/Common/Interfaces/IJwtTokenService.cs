using DPDP.Domain.Modules.Identity;

namespace DPDP.Application.Common.Interfaces;

public sealed record AccessTokenResult(string Token, DateTimeOffset ExpiresAt);

public interface IJwtTokenService
{
    /// <summary>
    /// permissions is the caller's fully resolved permission set (from all
    /// assigned roles); roleNames is used for display/claims only —
    /// authorization checks must always use permissions, never role names.
    /// </summary>
    AccessTokenResult GenerateAccessToken(
        User user,
        IReadOnlyCollection<string> roleNames,
        IReadOnlyCollection<string> permissions,
        bool isSuperAdministrator);
}
