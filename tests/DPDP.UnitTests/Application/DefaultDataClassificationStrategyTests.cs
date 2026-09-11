using DPDP.Application.Modules.DataDiscovery.Classification;
using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.Extensions.Options;
using Xunit;

namespace DPDP.UnitTests.Application;

public class DefaultDataClassificationStrategyTests
{
    private readonly DefaultDataClassificationStrategy _strategy = new(Options.Create(new ClassificationOptions()));

    [Theory]
    [InlineData("email", ClassificationCategory.CONTACT)]
    [InlineData("email_address", ClassificationCategory.CONTACT)]
    [InlineData("phone_number", ClassificationCategory.CONTACT)]
    [InlineData("ssn", ClassificationCategory.IDENTIFIER)]
    [InlineData("aadhaar_number", ClassificationCategory.IDENTIFIER)]
    [InlineData("account_number", ClassificationCategory.FINANCIAL)]
    [InlineData("card_number", ClassificationCategory.FINANCIAL)]
    [InlineData("employee_id", ClassificationCategory.EMPLOYEE)]
    [InlineData("customer_id", ClassificationCategory.CUSTOMER)]
    [InlineData("diagnosis", ClassificationCategory.HEALTH_RELATED)]
    [InlineData("latitude", ClassificationCategory.LOCATION)]
    [InlineData("street_address", ClassificationCategory.ADDRESS)]
    [InlineData("date_of_birth", ClassificationCategory.IDENTITY)]
    public void Recognizes_the_expected_category_from_the_column_name(string columnName, ClassificationCategory expected)
    {
        var result = _strategy.Classify(columnName, "varchar", null);

        Assert.Equal(expected, result.Category);
        Assert.True(result.Confidence > 0);
    }

    [Fact]
    public void An_exact_keyword_token_match_scores_higher_than_a_substring_match()
    {
        var exact = _strategy.Classify("email", "varchar", null);
        // No separator, so "email" is a substring of one token rather than
        // a whole token on its own — the weaker "contains" match tier.
        var substring = _strategy.Classify("customeremail", "varchar", null);

        Assert.True(exact.Confidence > substring.Confidence);
    }

    [Fact]
    public void A_column_name_with_no_keyword_match_returns_no_category_and_zero_confidence()
    {
        var result = _strategy.Classify("widget_count", "int", null);

        Assert.Null(result.Category);
        Assert.Equal(0m, result.Confidence);
    }

    [Fact]
    public void A_digit_shaped_masked_sample_boosts_confidence_for_an_identifier_column()
    {
        var withoutSample = _strategy.Classify("national_id", "varchar", null);
        var withDigitSample = _strategy.Classify("national_id", "varchar", "98******10");

        Assert.True(withDigitSample.Confidence > withoutSample.Confidence);
    }

    [Fact]
    public void A_non_digit_masked_sample_does_not_boost_confidence()
    {
        var withoutSample = _strategy.Classify("phone_number", "varchar", null);
        var withTextSample = _strategy.Classify("phone_number", "varchar", "ab****yz");

        Assert.Equal(withoutSample.Confidence, withTextSample.Confidence);
    }

    [Fact]
    public void Confidence_never_exceeds_ninety_nine()
    {
        var result = _strategy.Classify("ssn", "varchar", "12******89");
        Assert.True(result.Confidence <= 99m);
    }

    [Fact]
    public void Other_personal_data_is_never_auto_assigned()
    {
        // OTHER_PERSONAL_DATA has no keyword list by design — it exists only
        // for a human reviewer to apply, see ClassificationOptions.
        foreach (var name in new[] { "misc_field", "notes", "comment", "extra_data" })
        {
            var result = _strategy.Classify(name, "varchar", null);
            Assert.NotEqual(ClassificationCategory.OTHER_PERSONAL_DATA, result.Category);
        }
    }
}
