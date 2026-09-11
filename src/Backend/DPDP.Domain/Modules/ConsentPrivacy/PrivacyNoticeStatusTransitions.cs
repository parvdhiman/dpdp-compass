namespace DPDP.Domain.Modules.ConsentPrivacy;

/// <summary>Strictly linear — no reopen/branch, unlike Assessment/ProcessingActivity — since a published notice's content should never be silently edited; a correction is a new version (new PrivacyNotice row, same Code, new Version).</summary>
public static class PrivacyNoticeStatusTransitions
{
    private static readonly Dictionary<PrivacyNoticeStatus, PrivacyNoticeStatus[]> Allowed = new()
    {
        [PrivacyNoticeStatus.DRAFT] = [PrivacyNoticeStatus.APPROVED],
        [PrivacyNoticeStatus.APPROVED] = [PrivacyNoticeStatus.PUBLISHED],
        [PrivacyNoticeStatus.PUBLISHED] = [PrivacyNoticeStatus.ARCHIVED],
        [PrivacyNoticeStatus.ARCHIVED] = [],
    };

    public static bool CanTransition(PrivacyNoticeStatus from, PrivacyNoticeStatus to) =>
        from == to || (Allowed.TryGetValue(from, out var targets) && targets.Contains(to));

    public static bool IsTerminal(PrivacyNoticeStatus status) => status == PrivacyNoticeStatus.ARCHIVED;
}
