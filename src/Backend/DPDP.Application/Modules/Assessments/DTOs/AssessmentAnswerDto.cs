using DPDP.Application.Modules.Compliance.DTOs;

namespace DPDP.Application.Modules.Assessments.DTOs;

public sealed record AssessmentAnswerDto(
    Guid AssessmentControlQuestionId,
    Guid QuestionId,
    string QuestionCode,
    string QuestionText,
    string? HelpText,
    string QuestionType,
    IReadOnlyList<string>? Options,
    bool IsRequired,
    int SortOrder,
    IReadOnlyList<EvidenceRequirementDto> EvidenceRequirements,
    Guid AnswerId,
    string Status,
    string? AnswerValue,
    IReadOnlyList<string>? AnswerValues,
    string? Comment,
    IReadOnlyList<EvidenceReferenceDto> Evidence,
    Guid? ReviewerId,
    string? ReviewerName,
    DateTimeOffset? ReviewedAt,
    string? ReviewComment,
    string? Confidence,
    string? AssessedRiskLevel,
    string? RemediationNotes);

public sealed record AssessmentControlQuestionnaireDto(
    Guid AssessmentControlId,
    Guid ControlId,
    string ControlBusinessId,
    string ControlName,
    string CategoryName,
    string RiskLevel,
    string Status,
    string? Notes,
    IReadOnlyList<AssessmentAnswerDto> Questions);
