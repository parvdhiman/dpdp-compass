using DPDP.Domain.Modules.Compliance;
using Microsoft.Extensions.Options;

namespace DPDP.Application.Modules.Assessments.Scoring;

/// <summary>
/// The default IComplianceScoringStrategy — see docs/SCORING.md for the
/// full worked explanation and rationale. Summary of the formula:
///
///   Control Score        = average PASS/PARTIAL/FAIL point value across
///                           every *applicable* (non-N/A) control.
///   Risk-Adjusted Score   = the same average, but weighted by each
///                           control's risk-level weight — a failed
///                           CRITICAL control hurts more than a failed LOW
///                           one.
///   Assessment Coverage   = required questions answered ÷ required
///                           questions total, across applicable controls.
///   Evidence Coverage     = mandatory evidence requirements satisfied ÷
///                           mandatory evidence requirements total, across
///                           applicable controls.
///   Overall Score         = Risk-Adjusted Score × Assessment Coverage ×
///                           Evidence Coverage — mirrors MASTER_PROMPT
///                           section 11's example (Control Effectiveness ×
///                           Risk Weight × Evidence Confidence × Assessment
///                           Coverage), folding "control effectiveness" and
///                           "risk weight" into the one Risk-Adjusted Score
///                           term above rather than double-counting control
///                           performance twice.
///
/// N/A handling ("vacuous truth"): a dimension with nothing to measure
/// (e.g. every control marked NOT_APPLICABLE, or zero mandatory evidence
/// requirements exist) reports 100, not null and not 0 — "nothing failed
/// among what applies" rather than "unknown" or "everything failed." This
/// is a deliberate, documented product choice, exercised directly by this
/// module's N/A-handling unit tests.
/// </summary>
public sealed class DefaultComplianceScoringStrategy(IOptions<ScoringOptions> options) : IComplianceScoringStrategy
{
    private readonly ScoringOptions _options = options.Value;

    public AssessmentScoreResult Calculate(IReadOnlyList<ScoringControlInput> controls)
    {
        var applicable = controls.Where(c => c.Status != AnswerStatus.NOT_APPLICABLE).ToList();

        var controlScore = AveragePoints(applicable);
        var riskAdjustedScore = WeightedAveragePoints(applicable);
        var assessmentCoverage = Coverage(
            applicable.Sum(c => c.AnsweredRequiredQuestionCount),
            applicable.Sum(c => c.RequiredQuestionCount));
        var evidenceCoverage = Coverage(
            applicable.Sum(c => c.SatisfiedMandatoryEvidenceCount),
            applicable.Sum(c => c.MandatoryEvidenceRequirementCount));

        var overallScore = riskAdjustedScore * (assessmentCoverage / 100.0) * (evidenceCoverage / 100.0);

        return new AssessmentScoreResult(overallScore, controlScore, riskAdjustedScore, evidenceCoverage, assessmentCoverage);
    }

    private double AveragePoints(IReadOnlyList<ScoringControlInput> applicable)
    {
        if (applicable.Count == 0)
        {
            return 100.0;
        }

        return applicable.Average(c => PointsFor(c.Status)) * 100.0;
    }

    private double WeightedAveragePoints(IReadOnlyList<ScoringControlInput> applicable)
    {
        if (applicable.Count == 0)
        {
            return 100.0;
        }

        var totalWeight = applicable.Sum(c => WeightFor(c.RiskLevel));
        if (totalWeight <= 0)
        {
            return 100.0;
        }

        var weightedSum = applicable.Sum(c => PointsFor(c.Status) * WeightFor(c.RiskLevel));
        return weightedSum / totalWeight * 100.0;
    }

    private static double Coverage(int satisfied, int total) =>
        total <= 0 ? 100.0 : (double)satisfied / total * 100.0;

    private double PointsFor(AnswerStatus status) => status switch
    {
        AnswerStatus.PASS => _options.StatusPointsPass,
        AnswerStatus.PARTIAL => _options.StatusPointsPartial,
        AnswerStatus.FAIL => _options.StatusPointsFail,
        AnswerStatus.NEEDS_REVIEW => _options.StatusPointsNeedsReview,
        AnswerStatus.NOT_ASSESSED => _options.StatusPointsNotAssessed,
        AnswerStatus.NOT_APPLICABLE => throw new InvalidOperationException("NOT_APPLICABLE controls must be excluded before scoring."),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };

    private double WeightFor(RiskLevel riskLevel) => riskLevel switch
    {
        RiskLevel.LOW => _options.RiskWeightLow,
        RiskLevel.MEDIUM => _options.RiskWeightMedium,
        RiskLevel.HIGH => _options.RiskWeightHigh,
        RiskLevel.CRITICAL => _options.RiskWeightCritical,
        _ => throw new ArgumentOutOfRangeException(nameof(riskLevel), riskLevel, null),
    };
}
