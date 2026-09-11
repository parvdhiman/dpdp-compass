namespace DPDP.Domain.Modules.Evidence;

/// <summary>
/// Pure workflow-transition rules for EvidenceItem.Status — no database
/// access, unit-testable in isolation, the same pattern as
/// FindingStatusTransitions/RemediationStatusTransitions from Module 6.
/// Uploading a new version is deliberately NOT covered here — it resets
/// status to UPLOADED from any non-ARCHIVED status unconditionally (a
/// "restart the review cycle" action, handled directly in
/// UploadEvidenceVersionCommand), not a transition this table validates.
/// </summary>
public static class EvidenceStatusTransitions
{
    private static readonly Dictionary<EvidenceStatus, EvidenceStatus[]> Allowed = new()
    {
        [EvidenceStatus.UPLOADED] = [EvidenceStatus.UNDER_REVIEW],
        [EvidenceStatus.UNDER_REVIEW] = [EvidenceStatus.APPROVED, EvidenceStatus.REJECTED],
        [EvidenceStatus.APPROVED] = [EvidenceStatus.EXPIRED, EvidenceStatus.ARCHIVED],
        [EvidenceStatus.REJECTED] = [EvidenceStatus.ARCHIVED],
        [EvidenceStatus.EXPIRED] = [EvidenceStatus.ARCHIVED],
        [EvidenceStatus.ARCHIVED] = [],
    };

    public static bool CanTransition(EvidenceStatus from, EvidenceStatus to) =>
        from == to || Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}
