using DPDP.Domain.Modules.Evidence;
using Xunit;

namespace DPDP.UnitTests.Domain;

public class EvidenceStatusTransitionsTests
{
    [Theory]
    [InlineData(EvidenceStatus.UPLOADED, EvidenceStatus.UNDER_REVIEW)]
    [InlineData(EvidenceStatus.UNDER_REVIEW, EvidenceStatus.APPROVED)]
    [InlineData(EvidenceStatus.UNDER_REVIEW, EvidenceStatus.REJECTED)]
    [InlineData(EvidenceStatus.APPROVED, EvidenceStatus.EXPIRED)]
    [InlineData(EvidenceStatus.APPROVED, EvidenceStatus.ARCHIVED)]
    [InlineData(EvidenceStatus.REJECTED, EvidenceStatus.ARCHIVED)]
    [InlineData(EvidenceStatus.EXPIRED, EvidenceStatus.ARCHIVED)]
    public void Documented_forward_transitions_are_allowed(EvidenceStatus from, EvidenceStatus to)
    {
        Assert.True(EvidenceStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(EvidenceStatus.UPLOADED, EvidenceStatus.APPROVED)]
    [InlineData(EvidenceStatus.UPLOADED, EvidenceStatus.REJECTED)]
    [InlineData(EvidenceStatus.UPLOADED, EvidenceStatus.ARCHIVED)]
    [InlineData(EvidenceStatus.UNDER_REVIEW, EvidenceStatus.UPLOADED)]
    [InlineData(EvidenceStatus.UNDER_REVIEW, EvidenceStatus.EXPIRED)]
    [InlineData(EvidenceStatus.APPROVED, EvidenceStatus.UPLOADED)]
    [InlineData(EvidenceStatus.APPROVED, EvidenceStatus.UNDER_REVIEW)]
    [InlineData(EvidenceStatus.APPROVED, EvidenceStatus.REJECTED)]
    [InlineData(EvidenceStatus.REJECTED, EvidenceStatus.APPROVED)]
    [InlineData(EvidenceStatus.REJECTED, EvidenceStatus.UNDER_REVIEW)]
    [InlineData(EvidenceStatus.EXPIRED, EvidenceStatus.APPROVED)]
    [InlineData(EvidenceStatus.EXPIRED, EvidenceStatus.UNDER_REVIEW)]
    public void Skipping_steps_or_moving_backward_is_rejected(EvidenceStatus from, EvidenceStatus to)
    {
        Assert.False(EvidenceStatusTransitions.CanTransition(from, to));
    }

    [Fact]
    public void Archived_is_terminal_with_no_further_transitions()
    {
        foreach (var target in Enum.GetValues<EvidenceStatus>().Where(s => s != EvidenceStatus.ARCHIVED))
        {
            Assert.False(EvidenceStatusTransitions.CanTransition(EvidenceStatus.ARCHIVED, target));
        }
    }

    [Theory]
    [InlineData(EvidenceStatus.UPLOADED)]
    [InlineData(EvidenceStatus.UNDER_REVIEW)]
    [InlineData(EvidenceStatus.APPROVED)]
    [InlineData(EvidenceStatus.REJECTED)]
    [InlineData(EvidenceStatus.EXPIRED)]
    [InlineData(EvidenceStatus.ARCHIVED)]
    public void A_status_can_always_transition_to_itself(EvidenceStatus status)
    {
        Assert.True(EvidenceStatusTransitions.CanTransition(status, status));
    }
}
