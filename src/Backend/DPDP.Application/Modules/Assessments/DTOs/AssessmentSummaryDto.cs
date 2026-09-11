namespace DPDP.Application.Modules.Assessments.DTOs;

public sealed record AssessmentSummaryDto(
    Guid Id,
    string Name,
    string Status,
    Guid FrameworkVersionId,
    string FrameworkName,
    string FrameworkVersionLabel,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    DateOnly? DueDate,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? DecidedAt,
    DateTimeOffset CreatedAt,
    int TotalControls,
    int CompletedControls);
