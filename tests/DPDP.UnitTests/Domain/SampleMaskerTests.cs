using DPDP.Domain.Modules.DataDiscovery;
using Xunit;

namespace DPDP.UnitTests.Domain;

public class SampleMaskerTests
{
    [Fact]
    public void Matches_the_brief_example()
    {
        Assert.Equal("98******10", SampleMasker.Mask("9876543210"));
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("ABCD")]
    public void Values_of_four_characters_or_fewer_are_fully_masked(string raw)
    {
        Assert.Equal(new string('*', raw.Length), SampleMasker.Mask(raw));
    }

    [Fact]
    public void A_five_character_value_shows_first_two_and_last_two_with_one_asterisk_between()
    {
        Assert.Equal("AB*DE", SampleMasker.Mask("ABCDE"));
    }

    [Fact]
    public void An_email_keeps_only_its_first_two_and_last_two_characters_visible()
    {
        const string raw = "john.doe@example.com";
        var expected = raw[..2] + new string('*', raw.Length - 4) + raw[^2..];
        Assert.Equal(expected, SampleMasker.Mask(raw));
    }

    [Fact]
    public void Null_stays_null()
    {
        Assert.Null(SampleMasker.Mask(null));
    }

    [Fact]
    public void Empty_stays_empty()
    {
        Assert.Equal(string.Empty, SampleMasker.Mask(string.Empty));
    }

    [Fact]
    public void Whitespace_only_value_is_trimmed_to_empty()
    {
        Assert.Equal(string.Empty, SampleMasker.Mask("   "));
    }

    [Fact]
    public void Never_reveals_more_than_four_characters_regardless_of_length()
    {
        var masked = SampleMasker.Mask("a-very-long-raw-personal-data-value-1234567890");
        var visibleChars = masked!.Replace("*", string.Empty);
        Assert.Equal(4, visibleChars.Length);
    }
}
