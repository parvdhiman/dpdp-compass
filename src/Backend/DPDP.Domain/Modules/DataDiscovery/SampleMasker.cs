namespace DPDP.Domain.Modules.DataDiscovery;

/// <summary>
/// The only function in this codebase allowed to see a raw sample value
/// from a customer data source — every connector must run a value through
/// this before it leaves connector code, so no raw personal data ever
/// reaches an Application-layer handler, a log line, or a database row.
/// See docs/DATA_DISCOVERY.md section 3.
/// </summary>
public static class SampleMasker
{
    private const int VisiblePrefixLength = 2;
    private const int VisibleSuffixLength = 2;

    /// <summary>
    /// "9876543210" → "98******10". Short values (4 characters or fewer)
    /// are masked completely rather than partially, since showing 2+2
    /// characters of a 4-character value discloses all of it.
    /// </summary>
    public static string? Mask(string? raw)
    {
        if (raw is null)
        {
            return null;
        }

        var trimmed = raw.Trim();
        if (trimmed.Length == 0)
        {
            return trimmed;
        }

        if (trimmed.Length <= VisiblePrefixLength + VisibleSuffixLength)
        {
            return new string('*', trimmed.Length);
        }

        var prefix = trimmed[..VisiblePrefixLength];
        var suffix = trimmed[^VisibleSuffixLength..];
        var maskedMiddleLength = trimmed.Length - VisiblePrefixLength - VisibleSuffixLength;
        return prefix + new string('*', maskedMiddleLength) + suffix;
    }
}
