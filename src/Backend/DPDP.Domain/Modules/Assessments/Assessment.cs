using DPDP.Domain.Common;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.Assessments;

/// <summary>
/// The aggregate root for Module 5 — a structured DPDP compliance
/// assessment run by one organisation against one FrameworkVersion.
/// AssessmentControl/AssessmentControlQuestion/AssessmentAnswer rows are
/// snapshotted from the control library at creation time (see
/// CreateAssessmentCommand) so the assessment stays stable even if the
/// control library changes later — the same "snapshot, don't live-join"
/// principle Module 4 already uses for Control.Version.
/// </summary>
public sealed class Assessment : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }

    public Guid FrameworkVersionId { get; set; }
    public FrameworkVersion FrameworkVersion { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssessmentStatus Status { get; set; } = AssessmentStatus.DRAFT;

    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }
    public Guid? SubmittedBy { get; set; }

    /// <summary>Denormalized from the latest AssessmentApproval row, so the Assessment List doesn't need a join to show the final decision.</summary>
    public DateTimeOffset? DecidedAt { get; set; }
    public Guid? DecidedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<AssessmentScope> Scopes { get; set; } = new List<AssessmentScope>();
    public ICollection<AssessmentControl> Controls { get; set; } = new List<AssessmentControl>();
    public ICollection<AssessmentReview> Reviews { get; set; } = new List<AssessmentReview>();
    public ICollection<AssessmentApproval> Approvals { get; set; } = new List<AssessmentApproval>();
}
