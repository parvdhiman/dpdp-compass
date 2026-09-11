namespace DPDP.Application.Modules.Risks.Scoring;

/// <summary>
/// Bound from configuration section "RiskScoring" — see appsettings.json.
/// "Make risk methodology configurable" (Module 6 brief): these weights
/// and thresholds can change per deployment without a code change, the
/// same convention Module 5's ScoringOptions established for compliance
/// scoring. See docs/RISK_METHODOLOGY.md.
/// </summary>
public sealed class RiskScoringOptions
{
    public const string SectionName = "RiskScoring";

    /// <summary>Relative weights of the four risk dimensions — need not sum to 1; normalized internally.</summary>
    public double LikelihoodWeight { get; set; } = 0.35;
    public double ImpactWeight { get; set; } = 0.35;
    public double DataSensitivityWeight { get; set; } = 0.15;
    public double ExposureWeight { get; set; } = 0.15;

    /// <summary>Score (0-100) at or above which a risk is bucketed into each band. Evaluated highest-first.</summary>
    public double CriticalThreshold { get; set; } = 75;
    public double HighThreshold { get; set; } = 50;
    public double MediumThreshold { get; set; } = 25;
}
