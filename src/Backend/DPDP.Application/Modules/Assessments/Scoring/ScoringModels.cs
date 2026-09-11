using DPDP.Domain.Modules.Compliance;

namespace DPDP.Application.Modules.Assessments.Scoring;

/// <summary>Per-control input to the scoring engine — everything a strategy needs, pre-aggregated so the strategy itself never touches the database.</summary>
public sealed record ScoringControlInput(
    AnswerStatus Status,
    RiskLevel RiskLevel,
    int RequiredQuestionCount,
    int AnsweredRequiredQuestionCount,
    int MandatoryEvidenceRequirementCount,
    int SatisfiedMandatoryEvidenceCount);

public sealed record AssessmentScoreResult(
    double? OverallScore,
    double? ControlScore,
    double? RiskAdjustedScore,
    double? EvidenceCoveragePercent,
    double? AssessmentCoveragePercent);
