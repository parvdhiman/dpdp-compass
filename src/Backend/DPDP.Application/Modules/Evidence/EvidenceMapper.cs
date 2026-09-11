using DPDP.Application.Modules.Evidence.DTOs;
using DPDP.Domain.Modules.Evidence;

namespace DPDP.Application.Modules.Evidence;

internal static class EvidenceMapper
{
    public static string DisplayNumber(EvidenceItem evidence) => $"EVID-{evidence.SequenceNumber:D5}";

    public static bool IsExpired(EvidenceItem evidence, DateOnly today) =>
        evidence.ExpiryDate is { } expiryDate && expiryDate < today && evidence.Status is EvidenceStatus.APPROVED or EvidenceStatus.EXPIRED;

    public static EvidenceVersionDto ToDto(EvidenceVersion version) => new(
        version.Id, version.VersionNumber, version.OriginalFileName, version.ContentType, version.SizeBytes,
        version.ChecksumSha256, version.ExternalUrl, version.MalwareScanStatus.ToString(),
        EvidenceFileValidator.IsPreviewSafe(version.ContentType), version.UploadedByUserId,
        version.UploadedByUser?.FullName ?? string.Empty, version.UploadedAt);

    public static EvidenceReviewRecordDto ToDto(EvidenceReviewRecord review) => new(
        review.Id, review.EvidenceVersionNumber, review.ReviewerUserId, review.Reviewer?.FullName ?? string.Empty,
        review.Decision.ToString(), review.Comments, review.CreatedAt);

    public static EvidenceSummaryDto ToSummaryDto(EvidenceItem evidence, DateOnly today) => new(
        evidence.Id, DisplayNumber(evidence), evidence.Title, evidence.EvidenceType.ToString(), evidence.Status.ToString(),
        evidence.OwnerUserId, evidence.Owner?.FullName, evidence.ReviewerUserId, evidence.Reviewer?.FullName,
        evidence.ExpiryDate, IsExpired(evidence, today),
        evidence.Versions.Count == 0 ? 0 : evidence.Versions.Max(v => v.VersionNumber), evidence.CreatedAt);

    public static EvidenceDetailDto ToDetailDto(EvidenceItem evidence, DateOnly today) => new(
        evidence.Id, DisplayNumber(evidence), evidence.Title, evidence.Description, evidence.EvidenceType.ToString(),
        evidence.Status.ToString(), evidence.AssessmentId, evidence.ControlId, evidence.Control?.ControlId, evidence.Control?.Name,
        evidence.FindingId, evidence.Finding is null ? null : $"FIND-{evidence.Finding.SequenceNumber:D5}",
        evidence.VendorReference, evidence.ProcessingActivityReference, evidence.OwnerUserId, evidence.Owner?.FullName,
        evidence.ReviewerUserId, evidence.Reviewer?.FullName, evidence.ExpiryDate, IsExpired(evidence, today),
        evidence.ApprovedAt, evidence.RejectedAt, evidence.RejectionReason, evidence.ArchivedAt,
        evidence.CreatedAt, evidence.UpdatedAt,
        evidence.Versions.OrderByDescending(v => v.VersionNumber).Select(ToDto).ToList(),
        evidence.Reviews.OrderByDescending(r => r.CreatedAt).Select(ToDto).ToList());
}
