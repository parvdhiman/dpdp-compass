using DPDP.Domain.Modules.ConsentPrivacy;
using Xunit;

namespace DPDP.UnitTests.Domain;

public class PrivacyNoticeStatusTransitionsTests
{
    [Theory]
    [InlineData(PrivacyNoticeStatus.DRAFT, PrivacyNoticeStatus.APPROVED)]
    [InlineData(PrivacyNoticeStatus.APPROVED, PrivacyNoticeStatus.PUBLISHED)]
    [InlineData(PrivacyNoticeStatus.PUBLISHED, PrivacyNoticeStatus.ARCHIVED)]
    public void Documented_forward_transitions_are_allowed(PrivacyNoticeStatus from, PrivacyNoticeStatus to)
    {
        Assert.True(PrivacyNoticeStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(PrivacyNoticeStatus.DRAFT, PrivacyNoticeStatus.PUBLISHED)]
    [InlineData(PrivacyNoticeStatus.DRAFT, PrivacyNoticeStatus.ARCHIVED)]
    [InlineData(PrivacyNoticeStatus.APPROVED, PrivacyNoticeStatus.ARCHIVED)]
    [InlineData(PrivacyNoticeStatus.APPROVED, PrivacyNoticeStatus.DRAFT)]
    [InlineData(PrivacyNoticeStatus.PUBLISHED, PrivacyNoticeStatus.DRAFT)]
    [InlineData(PrivacyNoticeStatus.ARCHIVED, PrivacyNoticeStatus.PUBLISHED)]
    public void There_is_no_reopen_or_skip_path(PrivacyNoticeStatus from, PrivacyNoticeStatus to)
    {
        Assert.False(PrivacyNoticeStatusTransitions.CanTransition(from, to));
    }

    [Fact]
    public void Archived_is_the_only_terminal_status()
    {
        foreach (var status in Enum.GetValues<PrivacyNoticeStatus>())
        {
            Assert.Equal(status == PrivacyNoticeStatus.ARCHIVED, PrivacyNoticeStatusTransitions.IsTerminal(status));
        }
    }

    [Fact]
    public void A_status_can_always_transition_to_itself()
    {
        foreach (var status in Enum.GetValues<PrivacyNoticeStatus>())
        {
            Assert.True(PrivacyNoticeStatusTransitions.CanTransition(status, status));
        }
    }
}
