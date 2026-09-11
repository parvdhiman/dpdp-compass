using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Risks;
using Microsoft.Extensions.Options;

namespace DPDP.Application.Modules.Risks.Scoring;

/// <summary>
/// Default risk methodology — see docs/RISK_METHODOLOGY.md for the full
/// explanation. Each of the four dimensions is normalized to 0-1 by its
/// ordinal position (Likelihood/Impact have 5 levels; DataSensitivity/
/// Exposure reuse Compliance.RiskLevel's 4 levels), combined via
/// configurable weights into a 0-100 score, then bucketed into a
/// RiskLevel via configurable thresholds.
/// </summary>
public sealed class DefaultRiskScoringStrategy(IOptions<RiskScoringOptions> options) : IRiskScoringStrategy
{
    private readonly RiskScoringOptions _options = options.Value;

    public RiskScoreResult Calculate(Likelihood likelihood, Impact impact, RiskLevel dataSensitivity, RiskLevel exposure)
    {
        var likelihoodNormalized = Normalize((int)likelihood, Enum.GetValues<Likelihood>().Length);
        var impactNormalized = Normalize((int)impact, Enum.GetValues<Impact>().Length);
        var dataSensitivityNormalized = Normalize((int)dataSensitivity, Enum.GetValues<RiskLevel>().Length);
        var exposureNormalized = Normalize((int)exposure, Enum.GetValues<RiskLevel>().Length);

        var totalWeight = _options.LikelihoodWeight + _options.ImpactWeight + _options.DataSensitivityWeight + _options.ExposureWeight;
        var weightedSum =
            likelihoodNormalized * _options.LikelihoodWeight +
            impactNormalized * _options.ImpactWeight +
            dataSensitivityNormalized * _options.DataSensitivityWeight +
            exposureNormalized * _options.ExposureWeight;

        var score = totalWeight <= 0 ? 0 : weightedSum / totalWeight * 100.0;

        var level = score switch
        {
            var s when s >= _options.CriticalThreshold => RiskLevel.CRITICAL,
            var s when s >= _options.HighThreshold => RiskLevel.HIGH,
            var s when s >= _options.MediumThreshold => RiskLevel.MEDIUM,
            _ => RiskLevel.LOW,
        };

        return new RiskScoreResult(score, level);
    }

    private static double Normalize(int ordinal, int levelCount) => levelCount <= 1 ? 0 : (double)ordinal / (levelCount - 1);
}
