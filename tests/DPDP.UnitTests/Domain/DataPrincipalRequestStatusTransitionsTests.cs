using DPDP.Domain.Modules.ConsentPrivacy;
using Xunit;

namespace DPDP.UnitTests.Domain;

public class DataPrincipalRequestStatusTransitionsTests
{
    [Theory]
    [InlineData(DataPrincipalRequestStatus.REQUESTED, DataPrincipalRequestStatus.IDENTITY_VERIFICATION)]
    [InlineData(DataPrincipalRequestStatus.REQUESTED, DataPrincipalRequestStatus.REJECTED)]
    [InlineData(DataPrincipalRequestStatus.IDENTITY_VERIFICATION, DataPrincipalRequestStatus.IN_PROGRESS)]
    [InlineData(DataPrincipalRequestStatus.IDENTITY_VERIFICATION, DataPrincipalRequestStatus.REJECTED)]
    [InlineData(DataPrincipalRequestStatus.IN_PROGRESS, DataPrincipalRequestStatus.AWAITING_INFORMATION)]
    [InlineData(DataPrincipalRequestStatus.IN_PROGRESS, DataPrincipalRequestStatus.COMPLETED)]
    [InlineData(DataPrincipalRequestStatus.IN_PROGRESS, DataPrincipalRequestStatus.REJECTED)]
    [InlineData(DataPrincipalRequestStatus.AWAITING_INFORMATION, DataPrincipalRequestStatus.IN_PROGRESS)]
    [InlineData(DataPrincipalRequestStatus.AWAITING_INFORMATION, DataPrincipalRequestStatus.REJECTED)]
    [InlineData(DataPrincipalRequestStatus.COMPLETED, DataPrincipalRequestStatus.CLOSED)]
    [InlineData(DataPrincipalRequestStatus.REJECTED, DataPrincipalRequestStatus.CLOSED)]
    public void Documented_forward_transitions_are_allowed(DataPrincipalRequestStatus from, DataPrincipalRequestStatus to)
    {
        Assert.True(DataPrincipalRequestStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(DataPrincipalRequestStatus.REQUESTED, DataPrincipalRequestStatus.IN_PROGRESS)]
    [InlineData(DataPrincipalRequestStatus.REQUESTED, DataPrincipalRequestStatus.COMPLETED)]
    [InlineData(DataPrincipalRequestStatus.IDENTITY_VERIFICATION, DataPrincipalRequestStatus.AWAITING_INFORMATION)]
    [InlineData(DataPrincipalRequestStatus.IN_PROGRESS, DataPrincipalRequestStatus.IDENTITY_VERIFICATION)]
    [InlineData(DataPrincipalRequestStatus.COMPLETED, DataPrincipalRequestStatus.IN_PROGRESS)]
    [InlineData(DataPrincipalRequestStatus.CLOSED, DataPrincipalRequestStatus.REQUESTED)]
    [InlineData(DataPrincipalRequestStatus.CLOSED, DataPrincipalRequestStatus.IN_PROGRESS)]
    public void Skipping_steps_or_leaving_the_terminal_status_is_rejected(DataPrincipalRequestStatus from, DataPrincipalRequestStatus to)
    {
        Assert.False(DataPrincipalRequestStatusTransitions.CanTransition(from, to));
    }

    [Fact]
    public void Closed_is_the_only_terminal_status()
    {
        foreach (var status in Enum.GetValues<DataPrincipalRequestStatus>())
        {
            Assert.Equal(status == DataPrincipalRequestStatus.CLOSED, DataPrincipalRequestStatusTransitions.IsTerminal(status));
        }
    }

    [Fact]
    public void A_status_can_always_transition_to_itself()
    {
        foreach (var status in Enum.GetValues<DataPrincipalRequestStatus>())
        {
            Assert.True(DataPrincipalRequestStatusTransitions.CanTransition(status, status));
        }
    }
}
