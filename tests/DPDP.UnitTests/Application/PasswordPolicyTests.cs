using DPDP.Application.Modules.Identity;
using Xunit;

namespace DPDP.UnitTests.Application;

public class PasswordPolicyTests
{
    private readonly PasswordPolicy _policy = new();

    [Fact]
    public void A_strong_password_passes()
    {
        var errors = _policy.Validate("Xk9!Zephyr*Batt3ry");

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("Sh0rt!")] // too short
    [InlineData("nouppercase1!")] // no uppercase
    [InlineData("NOLOWERCASE1!")] // no lowercase
    [InlineData("NoDigitsHere!")] // no digit
    [InlineData("NoSpecialChar123")] // no special character
    public void A_password_violating_one_rule_fails(string password)
    {
        var errors = _policy.Validate(password);

        Assert.NotEmpty(errors);
    }

    [Fact]
    public void A_password_containing_the_email_local_part_fails()
    {
        var errors = _policy.Validate("MySuperAdmin123!", "superadmin@dpdp-compass.local");

        Assert.Contains(errors, e => e.Contains("email", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Minimum_length_is_enforced()
    {
        // 11 characters, one under the minimum, but otherwise satisfies
        // every other rule — isolates the length check.
        const string justUnderMinimum = "Aa1!Aa1!Aa1";
        Assert.Equal(PasswordPolicy.MinimumLength - 1, justUnderMinimum.Length);

        var errors = _policy.Validate(justUnderMinimum);

        Assert.Contains(errors, e => e.Contains("12 characters", StringComparison.OrdinalIgnoreCase));
    }
}
