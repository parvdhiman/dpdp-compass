namespace DPDP.Application.Modules.Assessments.DTOs;

public sealed record AssessmentDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    Guid FrameworkVersionId,
    string FrameworkName,
    string FrameworkVersionLabel,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    DateOnly? DueDate,
    DateTimeOffset? SubmittedAt,
    Guid? SubmittedBy,
    DateTimeOffset? DecidedAt,
    Guid? DecidedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<AssessmentScopeDto> Scopes,
    IReadOnlyList<AssessmentControlSummaryDto> Controls,
    IReadOnlyList<AssessmentReviewDto> Reviews,
    IReadOnlyList<AssessmentApprovalDto> Approvals);
