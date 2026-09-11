namespace DPDP.Domain.Modules.Findings;

/// <summary>
/// Pure workflow-transition rules for Finding.Status — no database access,
/// unit-testable in isolation ("workflow transitions" is an explicit
/// Module 6 test requirement). ACCEPTED_RISK and CLOSED are reachable only
/// through their own dedicated commands (AcceptFindingRiskCommand /
/// CloseFindingCommand), not through the generic status-update path — see
/// docs/ARCHITECTURE.md Module 6 section.
/// </summary>
public static class FindingStatusTransitions
{
    private static readonly Dictionary<FindingStatus, FindingStatus[]> Allowed = new()
    {
        [FindingStatus.OPEN] = [FindingStatus.ASSIGNED, FindingStatus.ACCEPTED_RISK],
        [FindingStatus.ASSIGNED] = [FindingStatus.IN_PROGRESS, FindingStatus.ACCEPTED_RISK],
        [FindingStatus.IN_PROGRESS] = [FindingStatus.PENDING_VERIFICATION, FindingStatus.ACCEPTED_RISK],
        [FindingStatus.PENDING_VERIFICATION] = [FindingStatus.RESOLVED, FindingStatus.IN_PROGRESS, FindingStatus.ACCEPTED_RISK],
        [FindingStatus.RESOLVED] = [FindingStatus.CLOSED, FindingStatus.PENDING_VERIFICATION],
        [FindingStatus.CLOSED] = [],
        [FindingStatus.ACCEPTED_RISK] = [],
    };

    public static bool CanTransition(FindingStatus from, FindingStatus to) =>
        from == to || Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}
