using DPDP.Infrastructure.Security;

namespace DPDP.Api.Security;

public static class AuthorizationExtensions
{
    /// <summary>Maps to a dynamically built "Permission:&lt;key&gt;" policy — see PermissionPolicyProvider.</summary>
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, string permission) =>
        builder.RequireAuthorization($"{PermissionPolicyProvider.PolicyPrefix}{permission}");
}
