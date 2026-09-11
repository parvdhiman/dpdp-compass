using DPDP.Domain.Modules.Remediation;
using Xunit;

namespace DPDP.UnitTests.Domain;

public class RemediationStatusTransitionsTests
{
    [Theory]
    [InlineData(RemediationStatus.OPEN, RemediationStatus.IN_PROGRESS, true)]
    [InlineData(RemediationStatus.IN_PROGRESS, RemediationStatus.PENDING_VERIFICATION, true)]
    [InlineData(RemediationStatus.PENDING_VERIFICATION, RemediationStatus.VERIFIED, true)]
    [InlineData(RemediationStatus.PENDING_VERIFICATION, RemediationStatus.IN_PROGRESS, true)]
    [InlineData(RemediationStatus.VERIFIED, RemediationStatus.CLOSED, true)]
    public void Forward_and_documented_backward_transitions_are_allowed(RemediationStatus from, RemediationStatus to, bool expected)
    {
        Assert.Equal(expected, RemediationStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(RemediationStatus.OPEN, RemediationStatus.VERIFIED)]
    [InlineData(RemediationStatus.OPEN, RemediationStatus.CLOSED)]
    [InlineData(RemediationStatus.IN_PROGRESS, RemediationStatus.VERIFIED)]
    [InlineData(RemediationStatus.VERIFIED, RemediationStatus.OPEN)]
    [InlineData(RemediationStatus.CLOSED, RemediationStatus.OPEN)]
    public void Skipping_steps_or_leaving_a_terminal_status_is_rejected(RemediationStatus from, RemediationStatus to)
    {
        Assert.False(RemediationStatusTransitions.CanTransition(from, to));
    }

    [Fact]
    public void Closed_is_terminal()
    {
        foreach (var target in Enum.GetValues<RemediationStatus>().Where(s => s != RemediationStatus.CLOSED))
        {
            Assert.False(RemediationStatusTransitions.CanTransition(RemediationStatus.CLOSED, target));
        }
    }
}
