namespace DPDP.Domain.Modules.Findings;

/// <summary>ACCEPTED_RISK is reachable from any open status — see docs/ARCHITECTURE.md Module 6 section for the full transition table.</summary>
public enum FindingStatus
{
    OPEN,
    ASSIGNED,
    IN_PROGRESS,
    PENDING_VERIFICATION,
    RESOLVED,
    CLOSED,
    ACCEPTED_RISK,
}
