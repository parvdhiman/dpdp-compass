namespace DPDP.Domain.Modules.Remediation;

/// <summary>
/// Pure workflow-transition rules for RemediationTask.Status. VERIFIED is
/// reachable only through VerifyRemediationTaskCommand and CLOSED only
/// through CloseRemediationTaskCommand — not the generic status-update
/// path — see docs/ARCHITECTURE.md Module 6 section.
/// </summary>
public static class RemediationStatusTransitions
{
    private static readonly Dictionary<RemediationStatus, RemediationStatus[]> Allowed = new()
    {
        [RemediationStatus.OPEN] = [RemediationStatus.IN_PROGRESS],
        [RemediationStatus.IN_PROGRESS] = [RemediationStatus.PENDING_VERIFICATION],
        [RemediationStatus.PENDING_VERIFICATION] = [RemediationStatus.IN_PROGRESS, RemediationStatus.VERIFIED],
        [RemediationStatus.VERIFIED] = [RemediationStatus.CLOSED],
        [RemediationStatus.CLOSED] = [],
    };

    public static bool CanTransition(RemediationStatus from, RemediationStatus to) =>
        from == to || Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}
