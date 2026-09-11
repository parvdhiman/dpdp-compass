namespace DPDP.Application.Modules.Assessments.DTOs;

public sealed record AssessmentControlSummaryDto(
    Guid Id,
    Guid ControlId,
    string ControlBusinessId,
    string ControlName,
    string CategoryName,
    string RiskLevel,
    string Status,
    string? Notes,
    int QuestionCount,
    int AnsweredQuestionCount);
