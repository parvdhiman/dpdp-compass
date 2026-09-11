namespace DPDP.Application.Modules.Findings.DTOs;

public sealed record FindingSummaryDto(
    Guid Id,
    string FindingNumber,
    string Title,
    string Severity,
    string Status,
    string Source,
    Guid? OwnerUserId,
    string? OwnerName,
    DateOnly? DueDate,
    bool IsOverdue,
    string? RiskLevel,
    DateTimeOffset CreatedAt);

public sealed record FindingDetailDto(
    Guid Id,
    string FindingNumber,
    string Title,
    string Description,
    string Source,
    Guid? AssessmentId,
    Guid? AssessmentControlId,
    Guid? ControlId,
    string? ControlBusinessId,
    string? ControlName,
    string? AssetReference,
    Guid? RiskId,
    string? RiskNumber,
    string? RiskLevel,
    string Severity,
    Guid? OwnerUserId,
    string? OwnerName,
    DateOnly? DueDate,
    string Status,
    string? Recommendation,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<FindingRemediationTaskSummaryDto> RemediationTasks);

public sealed record FindingRemediationTaskSummaryDto(Guid Id, string Title, string Status, Guid? OwnerUserId, string? OwnerName, DateOnly? DueDate);
