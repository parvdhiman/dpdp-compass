namespace DPDP.Application.Modules.ConsentPrivacy.DTOs;

public sealed record DataPrincipalRequestSummaryDto(
    Guid Id, string RequestNumber, string RequestType, string Status, string RequesterName,
    Guid? AssignedToUserId, string? AssignedToName, DateTimeOffset? DueAt, bool IsOverdue, DateTimeOffset CreatedAt);

public sealed record DataPrincipalRequestDetailDto(
    Guid Id, string RequestNumber, string RequestType, string Status,
    string RequesterName, string? RequesterContactEmail, string? RequesterContactPhone, string? ExternalReferenceId,
    Guid? DataPrincipalId, string? DataPrincipalReference, Guid? RelatedConsentId, string? RelatedConsentNumber,
    string? Description,
    Guid? SlaPolicyId, string? SlaPolicyName, DateTimeOffset? DueAt, bool IsOverdue,
    DateTimeOffset? IdentityVerifiedAt, Guid? IdentityVerifiedBy,
    Guid? AssignedToUserId, string? AssignedToName,
    DateTimeOffset? ResolvedAt, string? ResolutionNotes, DateTimeOffset? RejectedAt, string? RejectionReason, DateTimeOffset? ClosedAt,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public sealed record SlaSummaryDto(int OpenCount, int OverdueCount, int DueWithin48HoursCount, int NoSlaConfiguredCount);
