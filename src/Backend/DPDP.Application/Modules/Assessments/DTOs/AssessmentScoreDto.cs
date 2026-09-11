namespace DPDP.Application.Modules.Assessments.DTOs;

/// <summary>
/// The five figures MASTER_PROMPT's introduction and Module 5's brief both
/// require the platform to show, instead of ever asserting a blunt "100%
/// compliant." Every percentage is 0-100. Null means "not computable"
/// (should not happen in practice — see docs/SCORING.md's "vacuous truth"
/// convention — but the type stays nullable so a future strategy is free
/// to report "not enough data" honestly rather than guessing).
/// </summary>
public sealed record AssessmentScoreDto(
    double? OverallScore,
    double? ControlScore,
    double? RiskAdjustedScore,
    double? EvidenceCoveragePercent,
    double? AssessmentCoveragePercent,
    int TotalControls,
    int ApplicableControls,
    int TotalRequiredQuestions,
    int AnsweredRequiredQuestions);
