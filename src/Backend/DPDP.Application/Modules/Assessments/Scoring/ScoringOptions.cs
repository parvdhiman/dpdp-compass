namespace DPDP.Application.Modules.Assessments.Scoring;

/// <summary>
/// Bound from configuration section "Scoring" — see appsettings.json. This
/// is the "configurable" half of MASTER_PROMPT section 11's "the exact
/// formula must be configurable": these numbers can change per deployment
/// without a code change or a redeploy of logic, only a config change.
/// The *shape* of the formula (which dimensions combine and how) lives in
/// IComplianceScoringStrategy instead, which is itself swappable via DI —
/// see docs/SCORING.md.
/// </summary>
public sealed class ScoringOptions
{
    public const string SectionName = "Scoring";

    /// <summary>Points awarded (0-1) for each control-status contributing to Control Score / Risk-Adjusted Score. NOT_APPLICABLE and NOT_ASSESSED are handled structurally, not via this table — see DefaultComplianceScoringStrategy.</summary>
    public double StatusPointsPass { get; set; } = 1.0;
    public double StatusPointsPartial { get; set; } = 0.5;
    public double StatusPointsFail { get; set; } = 0.0;
    public double StatusPointsNeedsReview { get; set; } = 0.0;
    public double StatusPointsNotAssessed { get; set; } = 0.0;

    /// <summary>Risk-severity weight multipliers used only by the Risk-Adjusted Score, so a failed CRITICAL control drags the score down harder than a failed LOW one.</summary>
    public double RiskWeightLow { get; set; } = 1.0;
    public double RiskWeightMedium { get; set; } = 2.0;
    public double RiskWeightHigh { get; set; } = 3.0;
    public double RiskWeightCritical { get; set; } = 4.0;
}
