using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Identity;

/// <summary>
/// A named, tenant-agnostic permission template — see docs/ARCHITECTURE.md
/// section 5. Module 2 only supports the fixed system roles in
/// <see cref="RoleNames"/>; custom role creation is not implemented.
/// </summary>
public sealed class Role : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
