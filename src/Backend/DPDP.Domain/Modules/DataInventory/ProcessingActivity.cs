using DPDP.Domain.Common;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.DataInventory;

/// <summary>
/// The Processing Activity Register entry — this platform's Record of
/// Processing Activities. DataSubjectCategoriesJson and
/// SecurityControlsJson are lightweight tag lists (the same
/// JSON-column-for-a-small-tag-set pattern as EvidenceVersion.MetadataJson
/// and DataAsset.IndexesJson), not separate manageable entities, since an
/// organisation doesn't need to curate a reusable catalogue of them the
/// way it does DataCategory/ItSystem/etc. Review workflow fields are
/// flattened onto this entity rather than modeled as separate child
/// entities (contrast Assessment's AssessmentReview/AssessmentApproval) —
/// a deliberate simplification since this workflow is a single
/// review→approve chain, not iterative multi-round review. See
/// docs/DATA_INVENTORY.md.
/// </summary>
public sealed class ProcessingActivity : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public int SequenceNumber { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string? DataSubjectCategoriesJson { get; set; }
    public string? SecurityControlsJson { get; set; }

    public Guid? RetentionPolicyId { get; set; }
    public RetentionPolicy? RetentionPolicy { get; set; }
    public Guid? OwnerUserId { get; set; }
    public User? Owner { get; set; }

    public ProcessingActivityStatus Status { get; set; } = ProcessingActivityStatus.DRAFT;
    public DateOnly? ReviewDate { get; set; }

    public DateTimeOffset? SubmittedForReviewAt { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public string? ReviewComments { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
    public Guid? ArchivedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<DataCategory> DataCategories { get; set; } = new List<DataCategory>();
    public ICollection<ItSystem> ItSystems { get; set; } = new List<ItSystem>();
    public ICollection<DataCollectionSource> DataCollectionSources { get; set; } = new List<DataCollectionSource>();
    public ICollection<Recipient> Recipients { get; set; } = new List<Recipient>();
    public ICollection<Processor> Processors { get; set; } = new List<Processor>();
}
