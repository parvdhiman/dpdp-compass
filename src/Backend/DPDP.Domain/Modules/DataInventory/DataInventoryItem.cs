using DPDP.Domain.Common;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.DataDiscovery;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.DataInventory;

/// <summary>
/// One catalogued type of personal data the organisation holds — the
/// "Data Inventory" record itself. DiscoveredDataElementId is an optional
/// bridge to Module 8: an inventory item can originate from an automated
/// discovery scan's DataElement, or be entered manually (most will be,
/// since Module 8 only scans registered DataSources) — see
/// docs/DATA_INVENTORY.md.
/// </summary>
public sealed class DataInventoryItem : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public int SequenceNumber { get; set; }

    public Guid DataCategoryId { get; set; }
    public DataCategory DataCategory { get; set; } = null!;
    public string DataElementName { get; set; } = string.Empty;
    public Guid? DiscoveredDataElementId { get; set; }
    public DataElement? DiscoveredDataElement { get; set; }
    public ClassificationCategory? Classification { get; set; }

    public Guid? DataCollectionSourceId { get; set; }
    public DataCollectionSource? DataCollectionSource { get; set; }
    public Guid? ItSystemId { get; set; }
    public ItSystem? ItSystem { get; set; }
    public Guid? OwnerUserId { get; set; }
    public User? Owner { get; set; }
    public string? Purpose { get; set; }
    public Guid? RetentionPolicyId { get; set; }
    public RetentionPolicy? RetentionPolicy { get; set; }
    public string? SharingDescription { get; set; }
    public Guid? ProcessorId { get; set; }
    public Processor? Processor { get; set; }
    public RiskLevel? RiskLevel { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
