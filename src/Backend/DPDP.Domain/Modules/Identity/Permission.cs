using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Identity;

/// <summary>A static catalogue row — see <see cref="PermissionKeys"/>.</summary>
public sealed class Permission : Entity
{
    public string Key { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
