namespace DPDP.Application.Modules.Evidence.DTOs;

public sealed record EvidenceReviewRecordDto(
    Guid Id,
    int EvidenceVersionNumber,
    Guid ReviewerUserId,
    string ReviewerName,
    string Decision,
    string? Comments,
    DateTimeOffset CreatedAt);
