namespace DPDP.Domain.Modules.Compliance;

/// <summary>
/// The verdict domain a future Assessment Engine module (Phase 2) will
/// record per control per organisation. Defined here, now, only as a
/// stable reference vocabulary — this module does not implement
/// assessment-taking itself. Exposed via GET /api/v1/compliance/reference/answer-statuses.
/// </summary>
public enum AnswerStatus
{
    PASS,
    PARTIAL,
    FAIL,
    NOT_APPLICABLE,
    NOT_ASSESSED,
    NEEDS_REVIEW,
}
