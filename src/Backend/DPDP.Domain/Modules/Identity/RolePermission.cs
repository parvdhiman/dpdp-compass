namespace DPDP.Domain.Modules.Identity;

/// <summary>Join entity: composite key (RoleId, PermissionId) — configured in RolePermissionConfiguration.</summary>
public sealed class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
