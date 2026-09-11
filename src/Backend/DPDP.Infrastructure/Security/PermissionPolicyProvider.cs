using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DPDP.Infrastructure.Security;

/// <summary>
/// Turns a policy name of the form "Permission:evidence.upload" into a
/// dynamically built policy — no attribute or endpoint has to pre-register
/// permissions with AddAuthorization, so adding a new permission check is
/// just a string, not a wiring change. See RequirePermission in the Api
/// project's endpoint helpers.
/// </summary>
public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    public const string PolicyPrefix = "Permission:";

    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PolicyPrefix, StringComparison.Ordinal))
        {
            var permission = policyName[PolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();
}
