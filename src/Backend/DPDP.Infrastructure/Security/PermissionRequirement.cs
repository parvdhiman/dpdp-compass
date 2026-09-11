using Microsoft.AspNetCore.Authorization;

namespace DPDP.Infrastructure.Security;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
