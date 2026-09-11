namespace DPDP.Application.Modules.Compliance.DTOs;

public sealed record ControlSummaryDto(
    Guid Id,
    string ControlId,
    string Name,
    string CategoryName,
    string RiskLevel,
    string Status,
    string ReviewStatus,
    int Version,
    string SourceReference,
    int QuestionCount);
