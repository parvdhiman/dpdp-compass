using DPDP.Domain.Modules.Findings;
using Xunit;

namespace DPDP.UnitTests.Domain;

public class FindingStatusTransitionsTests
{
    [Theory]
    [InlineData(FindingStatus.OPEN, FindingStatus.ASSIGNED, true)]
    [InlineData(FindingStatus.ASSIGNED, FindingStatus.IN_PROGRESS, true)]
    [InlineData(FindingStatus.IN_PROGRESS, FindingStatus.PENDING_VERIFICATION, true)]
    [InlineData(FindingStatus.PENDING_VERIFICATION, FindingStatus.RESOLVED, true)]
    [InlineData(FindingStatus.PENDING_VERIFICATION, FindingStatus.IN_PROGRESS, true)]
    [InlineData(FindingStatus.RESOLVED, FindingStatus.CLOSED, true)]
    public void Forward_and_documented_backward_transitions_are_allowed(FindingStatus from, FindingStatus to, bool expected)
    {
        Assert.Equal(expected, FindingStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(FindingStatus.OPEN, FindingStatus.ACCEPTED_RISK)]
    [InlineData(FindingStatus.ASSIGNED, FindingStatus.ACCEPTED_RISK)]
    [InlineData(FindingStatus.IN_PROGRESS, FindingStatus.ACCEPTED_RISK)]
    [InlineData(FindingStatus.PENDING_VERIFICATION, FindingStatus.ACCEPTED_RISK)]
    public void Accepted_risk_is_reachable_from_every_active_status(FindingStatus from, FindingStatus to)
    {
        Assert.True(FindingStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(FindingStatus.OPEN, FindingStatus.RESOLVED)]
    [InlineData(FindingStatus.OPEN, FindingStatus.CLOSED)]
    [InlineData(FindingStatus.ASSIGNED, FindingStatus.RESOLVED)]
    [InlineData(FindingStatus.IN_PROGRESS, FindingStatus.CLOSED)]
    [InlineData(FindingStatus.CLOSED, FindingStatus.OPEN)]
    [InlineData(FindingStatus.ACCEPTED_RISK, FindingStatus.OPEN)]
    [InlineData(FindingStatus.CLOSED, FindingStatus.ASSIGNED)]
    public void Skipping_steps_or_leaving_a_terminal_status_is_rejected(FindingStatus from, FindingStatus to)
    {
        Assert.False(FindingStatusTransitions.CanTransition(from, to));
    }

    [Fact]
    public void A_status_can_always_transition_to_itself()
    {
        Assert.True(FindingStatusTransitions.CanTransition(FindingStatus.IN_PROGRESS, FindingStatus.IN_PROGRESS));
    }

    [Fact]
    public void Closed_and_accepted_risk_are_terminal_with_no_further_transitions()
    {
        foreach (var target in Enum.GetValues<FindingStatus>().Where(s => s != FindingStatus.CLOSED))
        {
            Assert.False(FindingStatusTransitions.CanTransition(FindingStatus.CLOSED, target));
        }

        foreach (var target in Enum.GetValues<FindingStatus>().Where(s => s != FindingStatus.ACCEPTED_RISK))
        {
            Assert.False(FindingStatusTransitions.CanTransition(FindingStatus.ACCEPTED_RISK, target));
        }
    }
}
