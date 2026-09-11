using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Domain.Modules.ConsentPrivacy;

namespace DPDP.Application.Modules.ConsentPrivacy;

internal static class ConsentPrivacyMapper
{
    public static string NoticeNumber(PrivacyNotice notice) => $"PN-{notice.SequenceNumber:D5}";
    public static string ConsentNumber(ConsentRecord consent) => $"CR-{consent.SequenceNumber:D5}";
    public static string RequestNumber(DataPrincipalRequest request) => $"DPR-{request.SequenceNumber:D5}";

    public static DataPrincipalDto ToDto(DataPrincipal p) => new(
        p.Id, p.ExternalReferenceId, p.ReferenceCategory?.ToString(), p.Notes, p.IsActive, p.CreatedAt);

    public static ConsentPurposeDto ToDto(ConsentPurpose p) => new(
        p.Id, p.Name, p.Description, p.DataCategoryId, p.DataCategory?.Name, p.IsActive, p.CreatedAt);

    public static SlaPolicyDto ToDto(SlaPolicy p) => new(
        p.Id, p.Name, p.Description, p.RequestType?.ToString(), p.ResponseDueDays, p.IsActive, p.CreatedAt);

    public static PrivacyNoticeSummaryDto ToSummaryDto(PrivacyNotice n) => new(
        n.Id, NoticeNumber(n), n.Code, n.Title, n.Version, n.Language, n.Status.ToString(), n.PublishedDate, n.EffectiveDate, n.CreatedAt);

    public static PrivacyNoticeDetailDto ToDetailDto(PrivacyNotice n) => new(
        n.Id, NoticeNumber(n), n.Code, n.Title, n.Version, n.Language, n.Purpose, n.Status.ToString(),
        n.PublishedDate, n.EffectiveDate, n.ApprovedAt, n.PublishedAt, n.ArchivedAt, n.CreatedAt, n.UpdatedAt,
        n.DataCategories.Select(c => new DataCategoryRefDto(c.Id, c.Name)).ToList());

    public static ConsentRecordDto ToDto(ConsentRecord c) => new(
        c.Id, ConsentNumber(c), c.DataPrincipalId, c.DataPrincipal?.ExternalReferenceId ?? string.Empty,
        c.ConsentPurposeId, c.ConsentPurpose?.Name ?? string.Empty,
        c.NoticeVersionId, c.NoticeVersion is null ? null : $"{c.NoticeVersion.Code} v{c.NoticeVersion.Version}",
        c.GrantedAt, c.Channel.ToString(), c.Status.ToString(), c.WithdrawnAt, c.ExpiresAt,
        c.SourceSystem, c.ExternalReferenceId, c.CreatedAt, c.UpdatedAt);

    public static bool IsOverdue(DataPrincipalRequest request, IDateTimeProvider dateTimeProvider) =>
        request.DueAt is { } dueAt && dueAt < dateTimeProvider.UtcNow &&
        request.Status is not (DataPrincipalRequestStatus.COMPLETED or DataPrincipalRequestStatus.REJECTED or DataPrincipalRequestStatus.CLOSED);

    public static DataPrincipalRequestSummaryDto ToSummaryDto(DataPrincipalRequest r, IDateTimeProvider dateTimeProvider) => new(
        r.Id, RequestNumber(r), r.RequestType.ToString(), r.Status.ToString(), r.RequesterName,
        r.AssignedToUserId, r.AssignedToUser?.FullName, r.DueAt, IsOverdue(r, dateTimeProvider), r.CreatedAt);

    public static DataPrincipalRequestDetailDto ToDetailDto(DataPrincipalRequest r, IDateTimeProvider dateTimeProvider) => new(
        r.Id, RequestNumber(r), r.RequestType.ToString(), r.Status.ToString(),
        r.RequesterName, r.RequesterContactEmail, r.RequesterContactPhone, r.ExternalReferenceId,
        r.DataPrincipalId, r.DataPrincipal?.ExternalReferenceId, r.RelatedConsentId,
        r.RelatedConsent is null ? null : ConsentNumber(r.RelatedConsent),
        r.Description,
        r.SlaPolicyId, r.SlaPolicy?.Name, r.DueAt, IsOverdue(r, dateTimeProvider),
        r.IdentityVerifiedAt, r.IdentityVerifiedBy,
        r.AssignedToUserId, r.AssignedToUser?.FullName,
        r.ResolvedAt, r.ResolutionNotes, r.RejectedAt, r.RejectionReason, r.ClosedAt,
        r.CreatedAt, r.UpdatedAt);
}
