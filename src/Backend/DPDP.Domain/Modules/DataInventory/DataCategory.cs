using DPDP.Domain.Common;
using DPDP.Domain.Modules.DataDiscovery;

namespace DPDP.Domain.Modules.DataInventory;

/// <summary>
/// An organisation-managed grouping of personal data (e.g. "Contact
/// Details", "Financial Records") — shared master data referenced by both
/// DataInventoryItem and ProcessingActivity, per the brief's relationship
/// diagram. ClassificationCategory optionally links it to Module 8's
/// discovery taxonomy for consistency, but this entity is organisation
/// -managed and does not require a discovery scan to exist.
/// </summary>
public sealed class DataCategory : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ClassificationCategory? ClassificationCategory { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
