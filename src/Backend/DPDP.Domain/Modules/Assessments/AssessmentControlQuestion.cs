using DPDP.Domain.Common;
using ComplianceQuestion = DPDP.Domain.Modules.Compliance.AssessmentQuestion;

namespace DPDP.Domain.Modules.Assessments;

/// <summary>
/// The Module 5 brief names this entity "AssessmentQuestion," but that
/// name is already taken by the question-bank entity in
/// DPDP.Domain.Modules.Compliance (an entirely different concept: the
/// reusable library question vs. this row, which snapshots one question as
/// included in one specific assessment's control). Named
/// AssessmentControlQuestion here to avoid the collision — see
/// docs/ARCHITECTURE.md Module 5 section.
/// </summary>
public sealed class AssessmentControlQuestion : Entity, ITenantScoped
{
    public Guid OrganisationId { get; set; }

    public Guid AssessmentControlId { get; set; }
    public AssessmentControl AssessmentControl { get; set; } = null!;

    public Guid QuestionId { get; set; }
    public ComplianceQuestion Question { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public AssessmentAnswer? Answer { get; set; }
}
