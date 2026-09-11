using DPDP.Application.Modules.Risks.Scoring;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Risks;
using Microsoft.Extensions.Options;
using Xunit;

namespace DPDP.UnitTests.Application;

public class DefaultRiskScoringStrategyTests
{
    private static DefaultRiskScoringStrategy CreateStrategy(RiskScoringOptions? options = null) =>
        new(Options.Create(options ?? new RiskScoringOptions()));

    [Fact]
    public void Lowest_inputs_across_every_dimension_yield_the_lowest_score_and_LOW_level()
    {
        var strategy = CreateStrategy();

        var result = strategy.Calculate(Likelihood.RARE, Impact.NEGLIGIBLE, RiskLevel.LOW, RiskLevel.LOW);

        Assert.Equal(0, result.Score);
        Assert.Equal(RiskLevel.LOW, result.Level);
    }

    [Fact]
    public void Highest_inputs_across_every_dimension_yield_the_highest_score_and_CRITICAL_level()
    {
        var strategy = CreateStrategy();

        var result = strategy.Calculate(Likelihood.ALMOST_CERTAIN, Impact.SEVERE, RiskLevel.CRITICAL, RiskLevel.CRITICAL);

        Assert.Equal(100, result.Score);
        Assert.Equal(RiskLevel.CRITICAL, result.Level);
    }

    [Fact]
    public void A_higher_likelihood_alone_increases_the_score()
    {
        var strategy = CreateStrategy();

        var lower = strategy.Calculate(Likelihood.RARE, Impact.MODERATE, RiskLevel.MEDIUM, RiskLevel.MEDIUM);
        var higher = strategy.Calculate(Likelihood.ALMOST_CERTAIN, Impact.MODERATE, RiskLevel.MEDIUM, RiskLevel.MEDIUM);

        Assert.True(higher.Score > lower.Score);
    }

    [Fact]
    public void A_higher_impact_alone_increases_the_score()
    {
        var strategy = CreateStrategy();

        var lower = strategy.Calculate(Likelihood.POSSIBLE, Impact.NEGLIGIBLE, RiskLevel.MEDIUM, RiskLevel.MEDIUM);
        var higher = strategy.Calculate(Likelihood.POSSIBLE, Impact.SEVERE, RiskLevel.MEDIUM, RiskLevel.MEDIUM);

        Assert.True(higher.Score > lower.Score);
    }

    [Fact]
    public void Configurable_weights_change_which_dimension_dominates_the_score()
    {
        // All weight on Impact — Likelihood should then have no effect at all.
        var impactOnlyOptions = new RiskScoringOptions
        {
            LikelihoodWeight = 0,
            ImpactWeight = 1,
            DataSensitivityWeight = 0,
            ExposureWeight = 0,
        };
        var strategy = CreateStrategy(impactOnlyOptions);

        var lowLikelihood = strategy.Calculate(Likelihood.RARE, Impact.SEVERE, RiskLevel.LOW, RiskLevel.LOW);
        var highLikelihood = strategy.Calculate(Likelihood.ALMOST_CERTAIN, Impact.SEVERE, RiskLevel.LOW, RiskLevel.LOW);

        Assert.Equal(lowLikelihood.Score, highLikelihood.Score);
        Assert.Equal(100, lowLikelihood.Score);
    }

    [Fact]
    public void Configurable_thresholds_change_which_band_a_score_falls_into()
    {
        var lenientOptions = new RiskScoringOptions { CriticalThreshold = 99, HighThreshold = 90, MediumThreshold = 80 };
        var strategy = CreateStrategy(lenientOptions);

        // A moderate-ish input that would be MEDIUM/HIGH under defaults should now read as LOW under lenient thresholds.
        var result = strategy.Calculate(Likelihood.POSSIBLE, Impact.MODERATE, RiskLevel.MEDIUM, RiskLevel.MEDIUM);

        Assert.Equal(RiskLevel.LOW, result.Level);
    }
}
