namespace DPDP.Domain.Modules.Assessments;

/// <summary>
/// See docs/ARCHITECTURE.md Module 5 section for the full state machine
/// (which commands cause which transitions). ARCHIVED is reachable only
/// from APPROVED; REOPEN only returns from REJECTED to IN_PROGRESS.
/// </summary>
public enum AssessmentStatus
{
    DRAFT,
    IN_PROGRESS,
    SUBMITTED,
    UNDER_REVIEW,
    APPROVED,
    REJECTED,
    ARCHIVED,
}
