using DPDP.Domain.Modules.DataInventory;
using Xunit;

namespace DPDP.UnitTests.Domain;

public class ProcessingActivityStatusTransitionsTests
{
    [Theory]
    [InlineData(ProcessingActivityStatus.DRAFT, ProcessingActivityStatus.IN_REVIEW)]
    [InlineData(ProcessingActivityStatus.IN_REVIEW, ProcessingActivityStatus.APPROVED)]
    [InlineData(ProcessingActivityStatus.IN_REVIEW, ProcessingActivityStatus.DRAFT)]
    [InlineData(ProcessingActivityStatus.APPROVED, ProcessingActivityStatus.ARCHIVED)]
    [InlineData(ProcessingActivityStatus.APPROVED, ProcessingActivityStatus.DRAFT)]
    public void Documented_forward_and_reopen_transitions_are_allowed(ProcessingActivityStatus from, ProcessingActivityStatus to)
    {
        Assert.True(ProcessingActivityStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(ProcessingActivityStatus.DRAFT, ProcessingActivityStatus.APPROVED)]
    [InlineData(ProcessingActivityStatus.DRAFT, ProcessingActivityStatus.ARCHIVED)]
    [InlineData(ProcessingActivityStatus.IN_REVIEW, ProcessingActivityStatus.ARCHIVED)]
    [InlineData(ProcessingActivityStatus.ARCHIVED, ProcessingActivityStatus.DRAFT)]
    [InlineData(ProcessingActivityStatus.ARCHIVED, ProcessingActivityStatus.APPROVED)]
    public void Skipping_steps_or_leaving_the_terminal_status_is_rejected(ProcessingActivityStatus from, ProcessingActivityStatus to)
    {
        Assert.False(ProcessingActivityStatusTransitions.CanTransition(from, to));
    }

    [Fact]
    public void Archived_is_the_only_terminal_status()
    {
        foreach (var status in Enum.GetValues<ProcessingActivityStatus>())
        {
            Assert.Equal(status == ProcessingActivityStatus.ARCHIVED, ProcessingActivityStatusTransitions.IsTerminal(status));
        }
    }

    [Fact]
    public void A_status_can_always_transition_to_itself()
    {
        foreach (var status in Enum.GetValues<ProcessingActivityStatus>())
        {
            Assert.True(ProcessingActivityStatusTransitions.CanTransition(status, status));
        }
    }
}
