namespace DPDP.Application.Modules.Assessments.DTOs;

public sealed record AssessmentReviewDto(
    Guid Id,
    Guid ReviewerId,
    string ReviewerName,
    string Decision,
    string? Comments,
    DateTimeOffset CreatedAt);

public sealed record AssessmentApprovalDto(
    Guid Id,
    Guid DecidedByUserId,
    string DecidedByUserName,
    string Decision,
    string? Comments,
    DateTimeOffset CreatedAt);
