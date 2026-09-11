namespace DPDP.Domain.Modules.DataInventory;

/// <summary>
/// The brief's workflow — "Create, Draft, Review, Approve, Archive" —
/// maps to DRAFT (the state a new record starts in, "Create" being the
/// action, not a status) → IN_REVIEW → APPROVED → ARCHIVED, with
/// IN_REVIEW → DRAFT as a "send back for changes" escape hatch (the same
/// shape assessments use — see docs/ARCHITECTURE.md Module 5 section).
/// </summary>
public static class ProcessingActivityStatusTransitions
{
    private static readonly Dictionary<ProcessingActivityStatus, ProcessingActivityStatus[]> Allowed = new()
    {
        [ProcessingActivityStatus.DRAFT] = [ProcessingActivityStatus.IN_REVIEW],
        [ProcessingActivityStatus.IN_REVIEW] = [ProcessingActivityStatus.APPROVED, ProcessingActivityStatus.DRAFT],
        [ProcessingActivityStatus.APPROVED] = [ProcessingActivityStatus.ARCHIVED, ProcessingActivityStatus.DRAFT],
        [ProcessingActivityStatus.ARCHIVED] = [],
    };

    public static bool CanTransition(ProcessingActivityStatus from, ProcessingActivityStatus to) =>
        from == to || (Allowed.TryGetValue(from, out var targets) && targets.Contains(to));

    public static bool IsTerminal(ProcessingActivityStatus status) => status == ProcessingActivityStatus.ARCHIVED;
}
