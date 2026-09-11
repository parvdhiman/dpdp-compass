using System.Text.RegularExpressions;
using DPDP.Application.Common.Interfaces;

namespace DPDP.Application.Modules.Identity;

/// <summary>
/// Minimum 12 characters, at least one uppercase, lowercase, digit, and
/// special character; must not contain the local part of the account's own
/// email. Pure logic, no framework dependency — see docs/SECURITY.md.
/// </summary>
public sealed partial class PasswordPolicy : IPasswordPolicy
{
    public const int MinimumLength = 12;

    public IReadOnlyList<string> Validate(string password, string? email = null)
    {
        var errors = new List<string>();

        if (password.Length < MinimumLength)
        {
            errors.Add($"Password must be at least {MinimumLength} characters long.");
        }

        if (!UppercaseRegex().IsMatch(password))
        {
            errors.Add("Password must contain at least one uppercase letter.");
        }

        if (!LowercaseRegex().IsMatch(password))
        {
            errors.Add("Password must contain at least one lowercase letter.");
        }

        if (!DigitRegex().IsMatch(password))
        {
            errors.Add("Password must contain at least one digit.");
        }

        if (!SpecialCharacterRegex().IsMatch(password))
        {
            errors.Add("Password must contain at least one special character.");
        }

        var localPart = email?.Split('@').FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(localPart) &&
            password.Contains(localPart, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Password must not contain your email address.");
        }

        return errors;
    }

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UppercaseRegex();

    [GeneratedRegex("[a-z]")]
    private static partial Regex LowercaseRegex();

    [GeneratedRegex(@"\d")]
    private static partial Regex DigitRegex();

    [GeneratedRegex(@"[^A-Za-z0-9]")]
    private static partial Regex SpecialCharacterRegex();
}
