using DPDP.Domain.Common;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.Assessments;

/// <summary>
/// One row per review pass a reviewer makes while an Assessment is
/// SUBMITTED or UNDER_REVIEW — an append-only log, never edited or
/// deleted. Distinct from AssessmentApproval, which records only the
/// final formal approve/reject decision.
/// </summary>
public sealed class AssessmentReview : Entity, ITenantScoped
{
    public Guid OrganisationId { get; set; }

    public Guid AssessmentId { get; set; }
    public Assessment Assessment { get; set; } = null!;

    public Guid ReviewerId { get; set; }
    public User Reviewer { get; set; } = null!;

    public ReviewDecision Decision { get; set; }
    public string? Comments { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
