using DPDP.Domain.Common;
using DPDP.Domain.Modules.Compliance;

namespace DPDP.Domain.Modules.Assessments;

/// <summary>
/// One row per Control included in an Assessment, snapshotted from the
/// control library's ACTIVE controls under the chosen FrameworkVersion at
/// creation time. Status is a rollup computed from this control's
/// AssessmentAnswers by ControlStatusCalculator, persisted here (not
/// computed on every read) so listing/filtering/scoring don't need to
/// recompute it — recalculated by SaveAssessmentAnswerCommand every time
/// one of its answers changes. Reuses Compliance.AnswerStatus rather than
/// inventing a near-identical enum — see docs/ARCHITECTURE.md Module 5
/// section.
/// </summary>
public sealed class AssessmentControl : Entity, ITenantScoped
{
    public Guid OrganisationId { get; set; }

    public Guid AssessmentId { get; set; }
    public Assessment Assessment { get; set; } = null!;

    public Guid ControlId { get; set; }
    public Control Control { get; set; } = null!;

    public AnswerStatus Status { get; set; } = AnswerStatus.NOT_ASSESSED;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<AssessmentControlQuestion> Questions { get; set; } = new List<AssessmentControlQuestion>();
}
