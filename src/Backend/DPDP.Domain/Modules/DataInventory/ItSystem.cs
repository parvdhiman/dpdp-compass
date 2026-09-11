using DPDP.Domain.Common;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.DataInventory;

/// <summary>
/// Named "ItSystem" rather than the brief's literal "System" — "System"
/// collides with the .NET base namespace and would force every file that
/// uses it to fully qualify ordinary BCL types. See
/// docs/ARCHITECTURE.md Module 9 section.
/// </summary>
public sealed class ItSystem : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ItSystemType SystemType { get; set; }
    public Guid? OwnerUserId { get; set; }
    public User? Owner { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
