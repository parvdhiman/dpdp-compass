namespace DPDP.Application.Modules.Remediation.DTOs;

public sealed record EvidenceReferenceDto(string? Description, string? Url);

public sealed record RemediationCommentDto(Guid Id, Guid AuthorUserId, string AuthorName, string Comment, DateTimeOffset CreatedAt);

public sealed record RemediationTaskSummaryDto(
    Guid Id,
    Guid FindingId,
    string FindingNumber,
    string Title,
    string Status,
    Guid? OwnerUserId,
    string? OwnerName,
    DateOnly? DueDate,
    bool IsOverdue,
    DateTimeOffset CreatedAt);

public sealed record RemediationTaskDetailDto(
    Guid Id,
    Guid FindingId,
    string FindingNumber,
    string FindingTitle,
    string Title,
    string? Description,
    Guid? OwnerUserId,
    string? OwnerName,
    DateOnly? DueDate,
    string Status,
    bool IsOverdue,
    IReadOnlyList<EvidenceReferenceDto> Evidence,
    Guid? VerifiedByUserId,
    string? VerifiedByName,
    DateTimeOffset? VerifiedAt,
    string? VerificationNotes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<RemediationCommentDto> Comments);
