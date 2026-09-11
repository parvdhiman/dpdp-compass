using System.Text.Json;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Application.Modules.Assessments.Scoring;
using DPDP.Application.Modules.Compliance;
using DPDP.Domain.Modules.Assessments;
using DPDP.Domain.Modules.Compliance;

namespace DPDP.Application.Modules.Assessments;

internal static class AssessmentMapper
{
    public static List<EvidenceReferenceDto>? DeserializeEvidence(string? json) =>
        json is null ? null : JsonSerializer.Deserialize<List<EvidenceReferenceDto>>(json);

    public static string? SerializeEvidence(IReadOnlyList<EvidenceReferenceDto>? evidence) =>
        evidence is null || evidence.Count == 0 ? null : JsonSerializer.Serialize(evidence);

    public static List<string>? DeserializeAnswerValues(string? json) =>
        json is null ? null : JsonSerializer.Deserialize<List<string>>(json);

    public static string? SerializeAnswerValues(IReadOnlyList<string>? values) =>
        values is null || values.Count == 0 ? null : JsonSerializer.Serialize(values);

    public static AssessmentScopeDto ToDto(AssessmentScope scope) => new(
        scope.Id, scope.BusinessUnitId, scope.BusinessUnit?.Name, scope.DepartmentId, scope.Department?.Name, scope.Notes);

    public static AssessmentReviewDto ToDto(AssessmentReview review) => new(
        review.Id, review.ReviewerId, review.Reviewer?.FullName ?? string.Empty, review.Decision.ToString(),
        review.Comments, review.CreatedAt);

    public static AssessmentApprovalDto ToDto(AssessmentApproval approval) => new(
        approval.Id, approval.DecidedByUserId, approval.DecidedByUser?.FullName ?? string.Empty, approval.Decision.ToString(),
        approval.Comments, approval.CreatedAt);

    public static AssessmentControlSummaryDto ToSummaryDto(AssessmentControl control) => new(
        control.Id, control.ControlId, control.Control.ControlId, control.Control.Name, control.Control.ControlCategory.Name,
        control.Control.RiskLevel.ToString(), control.Status.ToString(), control.Notes,
        control.Questions.Count,
        control.Questions.Count(q => q.Answer is not null && q.Answer.Status != AnswerStatus.NOT_ASSESSED));

    public static AssessmentSummaryDto ToSummaryDto(Assessment assessment) => new(
        assessment.Id, assessment.Name, assessment.Status.ToString(), assessment.FrameworkVersionId,
        assessment.FrameworkVersion.Framework.Name, assessment.FrameworkVersion.VersionLabel,
        assessment.AssignedToUserId, assessment.AssignedToUser?.FullName, assessment.DueDate,
        assessment.SubmittedAt, assessment.DecidedAt, assessment.CreatedAt,
        assessment.Controls.Count,
        assessment.Controls.Count(c => c.Status != AnswerStatus.NOT_ASSESSED));

    public static AssessmentDetailDto ToDetailDto(Assessment assessment) => new(
        assessment.Id, assessment.Name, assessment.Description, assessment.Status.ToString(), assessment.FrameworkVersionId,
        assessment.FrameworkVersion.Framework.Name, assessment.FrameworkVersion.VersionLabel,
        assessment.AssignedToUserId, assessment.AssignedToUser?.FullName, assessment.DueDate,
        assessment.SubmittedAt, assessment.SubmittedBy, assessment.DecidedAt, assessment.DecidedBy,
        assessment.CreatedAt, assessment.UpdatedAt,
        assessment.Scopes.Select(ToDto).ToList(),
        assessment.Controls.Select(ToSummaryDto).ToList(),
        assessment.Reviews.OrderByDescending(r => r.CreatedAt).Select(ToDto).ToList(),
        assessment.Approvals.OrderByDescending(a => a.CreatedAt).Select(ToDto).ToList());

    public static AssessmentAnswerDto ToAnswerDto(AssessmentControlQuestion controlQuestion)
    {
        var question = controlQuestion.Question;
        var answer = controlQuestion.Answer!;

        return new AssessmentAnswerDto(
            controlQuestion.Id, question.Id, question.Code, question.Text, question.HelpText, question.QuestionType.ToString(),
            question.OptionsJson is null ? null : JsonSerializer.Deserialize<List<string>>(question.OptionsJson),
            question.IsRequired, question.SortOrder,
            question.EvidenceRequirements.Select(ComplianceMapper.ToDto).ToList(),
            answer.Id, answer.Status.ToString(), answer.AnswerValue, DeserializeAnswerValues(answer.AnswerValuesJson),
            answer.Comment, DeserializeEvidence(answer.EvidenceJson) ?? [],
            answer.ReviewerId, answer.Reviewer?.FullName, answer.ReviewedAt, answer.ReviewComment,
            answer.Confidence?.ToString(), answer.AssessedRiskLevel?.ToString(), answer.RemediationNotes);
    }

    public static AssessmentControlQuestionnaireDto ToQuestionnaireDto(AssessmentControl control) => new(
        control.Id, control.ControlId, control.Control.ControlId, control.Control.Name, control.Control.ControlCategory.Name,
        control.Control.RiskLevel.ToString(), control.Status.ToString(), control.Notes,
        control.Questions.OrderBy(q => q.Question.SortOrder).Select(ToAnswerDto).ToList());

    public static ScoringControlInput ToScoringInput(AssessmentControl control)
    {
        var requiredAnswers = control.Questions.Where(q => q.Question.IsRequired).ToList();
        var mandatoryEvidenceQuestions = control.Questions
            .Where(q => q.Question.EvidenceRequirements.Any(e => e.IsMandatory))
            .ToList();

        var satisfiedMandatoryEvidence = mandatoryEvidenceQuestions.Count(q =>
        {
            var evidence = DeserializeEvidence(q.Answer?.EvidenceJson);
            return evidence is { Count: > 0 };
        });

        return new ScoringControlInput(
            control.Status,
            control.Control.RiskLevel,
            requiredAnswers.Count,
            requiredAnswers.Count(q => q.Answer is not null && q.Answer.Status != AnswerStatus.NOT_ASSESSED),
            mandatoryEvidenceQuestions.Count,
            satisfiedMandatoryEvidence);
    }

    public static AssessmentScoreDto ToScoreDto(AssessmentScoreResult result, IReadOnlyList<AssessmentControl> controls)
    {
        var applicable = controls.Where(c => c.Status != AnswerStatus.NOT_APPLICABLE).ToList();
        var requiredQuestions = applicable.SelectMany(c => c.Questions).Where(q => q.Question.IsRequired).ToList();

        return new AssessmentScoreDto(
            result.OverallScore, result.ControlScore, result.RiskAdjustedScore,
            result.EvidenceCoveragePercent, result.AssessmentCoveragePercent,
            controls.Count, applicable.Count,
            requiredQuestions.Count,
            requiredQuestions.Count(q => q.Answer is not null && q.Answer.Status != AnswerStatus.NOT_ASSESSED));
    }
}
