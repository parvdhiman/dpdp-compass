using DPDP.Application.Modules.Assessments.Scoring;
using DPDP.Domain.Modules.Compliance;
using Microsoft.Extensions.Options;
using Xunit;

namespace DPDP.UnitTests.Application;

public class DefaultComplianceScoringStrategyTests
{
    private static DefaultComplianceScoringStrategy CreateStrategy() =>
        new(Options.Create(new ScoringOptions()));

    private static ScoringControlInput Control(
        AnswerStatus status,
        RiskLevel riskLevel = RiskLevel.MEDIUM,
        int requiredQuestionCount = 1,
        int answeredRequiredQuestionCount = 1,
        int mandatoryEvidenceRequirementCount = 0,
        int satisfiedMandatoryEvidenceCount = 0) =>
        new(status, riskLevel, requiredQuestionCount, answeredRequiredQuestionCount, mandatoryEvidenceRequirementCount, satisfiedMandatoryEvidenceCount);

    [Fact]
    public void All_controls_passing_yields_a_perfect_score_across_every_dimension()
    {
        var strategy = CreateStrategy();
        var controls = new[] { Control(AnswerStatus.PASS), Control(AnswerStatus.PASS) };

        var result = strategy.Calculate(controls);

        Assert.Equal(100.0, result.ControlScore);
        Assert.Equal(100.0, result.RiskAdjustedScore);
        Assert.Equal(100.0, result.AssessmentCoveragePercent);
        Assert.Equal(100.0, result.OverallScore);
    }

    [Fact]
    public void An_assessment_with_no_controls_at_all_scores_vacuously_at_100()
    {
        var strategy = CreateStrategy();

        var result = strategy.Calculate([]);

        Assert.Equal(100.0, result.ControlScore);
        Assert.Equal(100.0, result.RiskAdjustedScore);
        Assert.Equal(100.0, result.EvidenceCoveragePercent);
        Assert.Equal(100.0, result.AssessmentCoveragePercent);
        Assert.Equal(100.0, result.OverallScore);
    }

    [Fact]
    public void Controls_marked_not_applicable_are_excluded_rather_than_counted_as_failures()
    {
        var strategy = CreateStrategy();
        // One passing control and one N/A control — the N/A control must not drag the score down.
        var controls = new[] { Control(AnswerStatus.PASS), Control(AnswerStatus.NOT_APPLICABLE) };

        var result = strategy.Calculate(controls);

        Assert.Equal(100.0, result.ControlScore);
    }

    [Fact]
    public void When_every_control_is_not_applicable_the_score_is_vacuously_100_not_zero()
    {
        var strategy = CreateStrategy();
        var controls = new[] { Control(AnswerStatus.NOT_APPLICABLE), Control(AnswerStatus.NOT_APPLICABLE) };

        var result = strategy.Calculate(controls);

        Assert.Equal(100.0, result.ControlScore);
        Assert.Equal(100.0, result.RiskAdjustedScore);
        Assert.Equal(100.0, result.OverallScore);
    }

    [Fact]
    public void A_partial_answer_contributes_half_credit_to_the_control_score()
    {
        var strategy = CreateStrategy();
        var controls = new[] { Control(AnswerStatus.PASS), Control(AnswerStatus.PARTIAL) };

        var result = strategy.Calculate(controls);

        // (1.0 + 0.5) / 2 controls * 100
        Assert.Equal(75.0, result.ControlScore);
    }

    [Fact]
    public void A_failed_critical_control_drags_the_risk_adjusted_score_down_more_than_a_failed_low_one()
    {
        var strategy = CreateStrategy();

        var criticalFailure = strategy.Calculate([Control(AnswerStatus.FAIL, RiskLevel.CRITICAL), Control(AnswerStatus.PASS, RiskLevel.LOW)]);
        var lowFailure = strategy.Calculate([Control(AnswerStatus.FAIL, RiskLevel.LOW), Control(AnswerStatus.PASS, RiskLevel.CRITICAL)]);

        Assert.True(criticalFailure.RiskAdjustedScore < lowFailure.RiskAdjustedScore);
    }

    [Fact]
    public void Risk_adjusted_score_can_differ_from_the_plain_control_score()
    {
        var strategy = CreateStrategy();
        // A failed CRITICAL control alongside a passed LOW one: unweighted average is 50,
        // but the CRITICAL failure should pull the risk-adjusted average below 50.
        var controls = new[] { Control(AnswerStatus.FAIL, RiskLevel.CRITICAL), Control(AnswerStatus.PASS, RiskLevel.LOW) };

        var result = strategy.Calculate(controls);

        Assert.Equal(50.0, result.ControlScore);
        Assert.True(result.RiskAdjustedScore < result.ControlScore);
    }

    [Fact]
    public void Assessment_coverage_reflects_answered_over_total_required_questions()
    {
        var strategy = CreateStrategy();
        var controls = new[]
        {
            Control(AnswerStatus.NOT_ASSESSED, requiredQuestionCount: 4, answeredRequiredQuestionCount: 1),
        };

        var result = strategy.Calculate(controls);

        Assert.Equal(25.0, result.AssessmentCoveragePercent);
    }

    [Fact]
    public void Evidence_coverage_reflects_satisfied_over_total_mandatory_requirements()
    {
        var strategy = CreateStrategy();
        var controls = new[]
        {
            Control(AnswerStatus.PASS, mandatoryEvidenceRequirementCount: 2, satisfiedMandatoryEvidenceCount: 1),
        };

        var result = strategy.Calculate(controls);

        Assert.Equal(50.0, result.EvidenceCoveragePercent);
    }

    [Fact]
    public void No_mandatory_evidence_requirements_means_full_evidence_coverage_not_zero()
    {
        var strategy = CreateStrategy();
        var controls = new[] { Control(AnswerStatus.PASS, mandatoryEvidenceRequirementCount: 0) };

        var result = strategy.Calculate(controls);

        Assert.Equal(100.0, result.EvidenceCoveragePercent);
    }

    [Fact]
    public void Overall_score_is_penalized_by_incomplete_coverage_even_when_answered_controls_all_passed()
    {
        var strategy = CreateStrategy();
        var controls = new[]
        {
            Control(AnswerStatus.PASS, requiredQuestionCount: 2, answeredRequiredQuestionCount: 1, mandatoryEvidenceRequirementCount: 0),
        };

        var result = strategy.Calculate(controls);

        Assert.Equal(100.0, result.RiskAdjustedScore);
        Assert.Equal(50.0, result.AssessmentCoveragePercent);
        Assert.Equal(50.0, result.OverallScore);
    }

    [Fact]
    public void Configuration_changes_the_point_value_awarded_to_a_status()
    {
        var strategy = new DefaultComplianceScoringStrategy(Options.Create(new ScoringOptions { StatusPointsPartial = 0.75 }));
        var controls = new[] { Control(AnswerStatus.PARTIAL) };

        var result = strategy.Calculate(controls);

        Assert.Equal(75.0, result.ControlScore);
    }
}
