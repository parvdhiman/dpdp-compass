namespace DPDP.Application.Modules.Assessments.Scoring;

/// <summary>
/// The pluggable scoring engine docs/ARCHITECTURE.md section 6 anticipated
/// and MASTER_PROMPT section 11 requires ("do not use only Passed/Total...
/// create a configurable scoring engine... the exact formula must be
/// configurable"). Registered once in DPDP.Application's DI container; a
/// future deployment or module could swap in a different implementation
/// (e.g. a different combination formula) without touching any query
/// handler that calls it. See docs/SCORING.md for the documented default
/// formula.
/// </summary>
public interface IComplianceScoringStrategy
{
    AssessmentScoreResult Calculate(IReadOnlyList<ScoringControlInput> controls);
}
