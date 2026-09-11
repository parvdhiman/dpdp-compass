using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Risks;

namespace DPDP.Application.Modules.Risks.Scoring;

public sealed record RiskScoreResult(double Score, RiskLevel Level);

/// <summary>
/// The pluggable risk methodology docs/RISK_METHODOLOGY.md documents —
/// resolved via DI, swappable without touching any command/query handler,
/// mirroring IComplianceScoringStrategy from Module 5.
/// </summary>
public interface IRiskScoringStrategy
{
    RiskScoreResult Calculate(Likelihood likelihood, Impact impact, RiskLevel dataSensitivity, RiskLevel exposure);
}
