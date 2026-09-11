using DPDP.Domain.Common;
using DPDP.Domain.Modules.DataInventory;

namespace DPDP.Domain.Modules.ConsentPrivacy;

/// <summary>A specific purpose consent is sought for (e.g. "Marketing communications", "Analytics"). Optionally links to a Module 9 DataCategory for cross-module traceability of what kind of data the purpose covers.</summary>
public sealed class ConsentPurpose : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? DataCategoryId { get; set; }
    public DataCategory? DataCategory { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
