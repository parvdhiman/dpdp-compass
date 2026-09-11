namespace DPDP.Domain.Modules.DataDiscovery;

public static class DiscoveryJobStatusTransitions
{
    private static readonly Dictionary<DiscoveryJobStatus, DiscoveryJobStatus[]> Allowed = new()
    {
        [DiscoveryJobStatus.PENDING] = [DiscoveryJobStatus.RUNNING, DiscoveryJobStatus.CANCELLED],
        [DiscoveryJobStatus.RUNNING] = [DiscoveryJobStatus.COMPLETED, DiscoveryJobStatus.FAILED, DiscoveryJobStatus.CANCELLED],
        [DiscoveryJobStatus.COMPLETED] = [],
        [DiscoveryJobStatus.FAILED] = [],
        [DiscoveryJobStatus.CANCELLED] = [],
    };

    public static bool CanTransition(DiscoveryJobStatus from, DiscoveryJobStatus to) =>
        from == to || (Allowed.TryGetValue(from, out var targets) && targets.Contains(to));

    public static bool IsTerminal(DiscoveryJobStatus status) =>
        status is DiscoveryJobStatus.COMPLETED or DiscoveryJobStatus.FAILED or DiscoveryJobStatus.CANCELLED;
}
