using DPDP.Domain.Modules.ConsentPrivacy;
using Xunit;

namespace DPDP.UnitTests.Domain;

public class ConsentStatusTransitionsTests
{
    [Theory]
    [InlineData(ConsentStatus.GRANTED, ConsentStatus.WITHDRAWN)]
    [InlineData(ConsentStatus.GRANTED, ConsentStatus.EXPIRED)]
    [InlineData(ConsentStatus.GRANTED, ConsentStatus.REVOKED)]
    public void Granted_can_move_to_any_terminal_status(ConsentStatus from, ConsentStatus to)
    {
        Assert.True(ConsentStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(ConsentStatus.WITHDRAWN, ConsentStatus.GRANTED)]
    [InlineData(ConsentStatus.EXPIRED, ConsentStatus.GRANTED)]
    [InlineData(ConsentStatus.REVOKED, ConsentStatus.GRANTED)]
    [InlineData(ConsentStatus.WITHDRAWN, ConsentStatus.REVOKED)]
    public void Terminal_statuses_never_transition_anywhere_else(ConsentStatus from, ConsentStatus to)
    {
        Assert.False(ConsentStatusTransitions.CanTransition(from, to));
    }

    [Fact]
    public void Every_status_except_granted_is_terminal()
    {
        foreach (var status in Enum.GetValues<ConsentStatus>())
        {
            Assert.Equal(status != ConsentStatus.GRANTED, ConsentStatusTransitions.IsTerminal(status));
        }
    }

    [Fact]
    public void A_status_can_always_transition_to_itself()
    {
        foreach (var status in Enum.GetValues<ConsentStatus>())
        {
            Assert.True(ConsentStatusTransitions.CanTransition(status, status));
        }
    }
}
