using DPDP.Domain.Common;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.Assessments;

/// <summary>One row per final approve/reject decision — an append-only log. Assessment.DecidedAt/DecidedBy denormalize the latest row here for list-view display without a join.</summary>
public sealed class AssessmentApproval : Entity, ITenantScoped
{
    public Guid OrganisationId { get; set; }

    public Guid AssessmentId { get; set; }
    public Assessment Assessment { get; set; } = null!;

    public Guid DecidedByUserId { get; set; }
    public User DecidedByUser { get; set; } = null!;

    public ApprovalDecision Decision { get; set; }
    public string? Comments { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
