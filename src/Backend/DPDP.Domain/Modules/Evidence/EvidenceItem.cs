using DPDP.Domain.Common;
using DPDP.Domain.Modules.Assessments;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Findings;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.Evidence;

/// <summary>
/// Named "EvidenceItem," not "Evidence," purely to avoid colliding with
/// the existing DPDP.Domain.Modules.Compliance.EvidenceRequirement concept
/// and the word "evidence" already used as a field name in several other
/// entities — see docs/ARCHITECTURE.md Module 7 section.
///
/// "Evidence ID" is a computed display value (EVID-00001), the same
/// pattern as Finding/Risk in Module 6 — see EvidenceMapper.DisplayNumber.
/// The actual file (for every EvidenceType except URL) lives in object
/// storage, never in this database — see IObjectStorageService and
/// EvidenceVersion.StorageKey. Vendor/Processing Activity links are
/// free-text placeholders (VendorReference/ProcessingActivityReference)
/// until those modules exist (roadmap items 20/23), the same pattern
/// Module 6 used for Finding.AssetReference.
/// </summary>
public sealed class EvidenceItem : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public int SequenceNumber { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EvidenceType EvidenceType { get; set; }
    public EvidenceStatus Status { get; set; } = EvidenceStatus.UPLOADED;

    public Guid? AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

    public Guid? ControlId { get; set; }
    public Control? Control { get; set; }

    public Guid? FindingId { get; set; }
    public Finding? Finding { get; set; }

    public string? VendorReference { get; set; }
    public string? ProcessingActivityReference { get; set; }

    public Guid? OwnerUserId { get; set; }
    public User? Owner { get; set; }

    public Guid? ReviewerUserId { get; set; }
    public User? Reviewer { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
    public Guid? ArchivedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<EvidenceVersion> Versions { get; set; } = new List<EvidenceVersion>();
    public ICollection<EvidenceReviewRecord> Reviews { get; set; } = new List<EvidenceReviewRecord>();
}
