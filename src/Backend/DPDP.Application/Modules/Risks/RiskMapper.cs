using DPDP.Application.Modules.Risks.DTOs;
using DPDP.Domain.Modules.Risks;

namespace DPDP.Application.Modules.Risks;

internal static class RiskMapper
{
    public static string DisplayNumber(Risk risk) => $"RISK-{risk.SequenceNumber:D5}";

    public static RiskSummaryDto ToSummaryDto(Risk risk) => new(
        risk.Id, DisplayNumber(risk), risk.Title, risk.Likelihood.ToString(), risk.Impact.ToString(),
        risk.DataSensitivity.ToString(), risk.Exposure.ToString(), risk.CalculatedRiskLevel.ToString(),
        risk.CalculatedRiskScore, risk.Status.ToString(), risk.OwnerUserId, risk.Owner?.FullName,
        risk.CreatedAt, risk.Findings.Count);

    public static RiskDetailDto ToDetailDto(Risk risk) => new(
        risk.Id, DisplayNumber(risk), risk.Title, risk.Description, risk.Likelihood.ToString(), risk.Impact.ToString(),
        risk.DataSensitivity.ToString(), risk.Exposure.ToString(), risk.CalculatedRiskLevel.ToString(),
        risk.CalculatedRiskScore, risk.Status.ToString(), risk.TreatmentPlan, risk.OwnerUserId, risk.Owner?.FullName,
        risk.CreatedAt, risk.UpdatedAt,
        risk.Findings.Select(f => new LinkedFindingDto(f.Id, Findings.FindingMapper.DisplayNumber(f), f.Title, f.Severity.ToString(), f.Status.ToString())).ToList());
}
