namespace DPDP.Domain.Modules.Assessments;

/// <summary>
/// The assessor's self-reported confidence in a single answer — rolls up
/// into the platform-level "Assessment Confidence" concept MASTER_PROMPT's
/// introduction lists alongside Compliance Score, never a claim of legal
/// certainty.
/// </summary>
public enum ConfidenceLevel
{
    LOW,
    MEDIUM,
    HIGH,
}
