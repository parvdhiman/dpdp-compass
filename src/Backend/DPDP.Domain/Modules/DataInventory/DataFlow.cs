using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.DataInventory;

/// <summary>
/// Metadata describing a known movement of data from one point to
/// another (e.g. "web form → CRM → payment processor") — never an actual
/// data pipe; this module only records that a flow exists, for compliance
/// mapping (cross-border transfer visibility, recipient/processor
/// traceability). Each side is either a catalog reference (ItSystem/
/// DataCollectionSource for the "from" side, ItSystem/Processor/Recipient
/// for the "to" side) or, when no catalog entry fits, a free-text
/// description — see docs/DATA_INVENTORY.md.
/// </summary>
public sealed class DataFlow : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public int SequenceNumber { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid? ProcessingActivityId { get; set; }
    public ProcessingActivity? ProcessingActivity { get; set; }
    public Guid? DataCategoryId { get; set; }
    public DataCategory? DataCategory { get; set; }

    public Guid? FromItSystemId { get; set; }
    public ItSystem? FromItSystem { get; set; }
    public Guid? FromDataCollectionSourceId { get; set; }
    public DataCollectionSource? FromDataCollectionSource { get; set; }
    public string FromDescription { get; set; } = string.Empty;

    public Guid? ToItSystemId { get; set; }
    public ItSystem? ToItSystem { get; set; }
    public Guid? ToProcessorId { get; set; }
    public Processor? ToProcessor { get; set; }
    public Guid? ToRecipientId { get; set; }
    public Recipient? ToRecipient { get; set; }
    public string ToDescription { get; set; } = string.Empty;

    public string? TransferMechanism { get; set; }
    public bool IsCrossBorder { get; set; }
    public string? CrossBorderCountry { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
