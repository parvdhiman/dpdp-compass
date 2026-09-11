using DPDP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DPDP.Infrastructure.Security;

/// <summary>
/// Permission-based, never role-name-based — see docs/ARCHITECTURE.md
/// section 5. A Super Administrator satisfies every permission
/// requirement; everyone else must hold the specific permission claim.
/// </summary>
public sealed class PermissionAuthorizationHandler(ICurrentUserContext currentUser)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (currentUser.IsSuperAdministrator || currentUser.Permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
