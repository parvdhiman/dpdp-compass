namespace DPDP.Application.Modules.Risks.DTOs;

public sealed record RiskSummaryDto(
    Guid Id,
    string RiskNumber,
    string Title,
    string Likelihood,
    string Impact,
    string DataSensitivity,
    string Exposure,
    string CalculatedRiskLevel,
    double CalculatedRiskScore,
    string Status,
    Guid? OwnerUserId,
    string? OwnerName,
    DateTimeOffset CreatedAt,
    int FindingCount);

public sealed record RiskDetailDto(
    Guid Id,
    string RiskNumber,
    string Title,
    string Description,
    string Likelihood,
    string Impact,
    string DataSensitivity,
    string Exposure,
    string CalculatedRiskLevel,
    double CalculatedRiskScore,
    string Status,
    string? TreatmentPlan,
    Guid? OwnerUserId,
    string? OwnerName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<LinkedFindingDto> Findings);

public sealed record LinkedFindingDto(Guid Id, string FindingNumber, string Title, string Severity, string Status);
