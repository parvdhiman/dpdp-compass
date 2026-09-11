using DPDP.Domain.Modules.DataDiscovery;
using Xunit;

namespace DPDP.UnitTests.Domain;

public class DiscoveryJobStatusTransitionsTests
{
    [Theory]
    [InlineData(DiscoveryJobStatus.PENDING, DiscoveryJobStatus.RUNNING)]
    [InlineData(DiscoveryJobStatus.PENDING, DiscoveryJobStatus.CANCELLED)]
    [InlineData(DiscoveryJobStatus.RUNNING, DiscoveryJobStatus.COMPLETED)]
    [InlineData(DiscoveryJobStatus.RUNNING, DiscoveryJobStatus.FAILED)]
    [InlineData(DiscoveryJobStatus.RUNNING, DiscoveryJobStatus.CANCELLED)]
    public void Documented_forward_transitions_are_allowed(DiscoveryJobStatus from, DiscoveryJobStatus to)
    {
        Assert.True(DiscoveryJobStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(DiscoveryJobStatus.PENDING, DiscoveryJobStatus.COMPLETED)]
    [InlineData(DiscoveryJobStatus.PENDING, DiscoveryJobStatus.FAILED)]
    [InlineData(DiscoveryJobStatus.COMPLETED, DiscoveryJobStatus.RUNNING)]
    [InlineData(DiscoveryJobStatus.FAILED, DiscoveryJobStatus.RUNNING)]
    [InlineData(DiscoveryJobStatus.CANCELLED, DiscoveryJobStatus.RUNNING)]
    [InlineData(DiscoveryJobStatus.CANCELLED, DiscoveryJobStatus.PENDING)]
    public void Skipping_steps_or_leaving_a_terminal_status_is_rejected(DiscoveryJobStatus from, DiscoveryJobStatus to)
    {
        Assert.False(DiscoveryJobStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(DiscoveryJobStatus.COMPLETED, true)]
    [InlineData(DiscoveryJobStatus.FAILED, true)]
    [InlineData(DiscoveryJobStatus.CANCELLED, true)]
    [InlineData(DiscoveryJobStatus.PENDING, false)]
    [InlineData(DiscoveryJobStatus.RUNNING, false)]
    public void IsTerminal_matches_the_three_end_states(DiscoveryJobStatus status, bool expected)
    {
        Assert.Equal(expected, DiscoveryJobStatusTransitions.IsTerminal(status));
    }

    [Fact]
    public void A_status_can_always_transition_to_itself()
    {
        foreach (var status in Enum.GetValues<DiscoveryJobStatus>())
        {
            Assert.True(DiscoveryJobStatusTransitions.CanTransition(status, status));
        }
    }
}
