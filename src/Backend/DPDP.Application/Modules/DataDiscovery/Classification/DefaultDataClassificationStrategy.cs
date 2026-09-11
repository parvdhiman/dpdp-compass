using System.Text.RegularExpressions;
using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.Extensions.Options;

namespace DPDP.Application.Modules.DataDiscovery.Classification;

/// <summary>
/// Rule-based only — no ML, no external service, per the Module 8 brief's
/// "do not implement advanced AI". Reasons over the column name (the
/// dominant, reliable signal) and, weakly, over the shape of the already
/// -masked sample (the only sample data this classifier — or anything
/// else in this codebase — is ever allowed to see). Column-order in
/// CategoryOrder is the tie-break when a name matches more than one
/// category's keyword list, kept as an explicit fixed list (not dictionary
/// enumeration order) so the outcome is deterministic and reviewable.
/// See docs/DATA_DISCOVERY.md section 4.
/// </summary>
public sealed class DefaultDataClassificationStrategy(IOptions<ClassificationOptions> options) : IDataClassificationStrategy
{
    private static readonly ClassificationCategory[] CategoryOrder =
    [
        ClassificationCategory.IDENTIFIER,
        ClassificationCategory.FINANCIAL,
        ClassificationCategory.HEALTH_RELATED,
        ClassificationCategory.CHILD,
        ClassificationCategory.CONTACT,
        ClassificationCategory.ADDRESS,
        ClassificationCategory.LOCATION,
        ClassificationCategory.EMPLOYEE,
        ClassificationCategory.CUSTOMER,
        ClassificationCategory.IDENTITY,
    ];

    private static readonly Regex TokenSplitter = new("[^a-z0-9]+", RegexOptions.Compiled);
    private static readonly Regex DigitShapedVisibleChars = new("^[0-9+.\\- ]+$", RegexOptions.Compiled);

    private static readonly HashSet<ClassificationCategory> SamplePatternEligible =
    [
        ClassificationCategory.IDENTIFIER, ClassificationCategory.CONTACT,
        ClassificationCategory.FINANCIAL, ClassificationCategory.LOCATION,
    ];

    public ClassificationSuggestion Classify(string columnName, string dataType, string? sampleMaskedValue)
    {
        var options1 = options.Value;
        var normalizedName = columnName.Trim().ToLowerInvariant();
        var tokens = TokenSplitter.Split(normalizedName).Where(t => t.Length > 0).ToHashSet();

        ClassificationCategory? bestCategory = null;
        var bestConfidence = 0m;

        foreach (var category in CategoryOrder)
        {
            if (!options1.ColumnNameKeywords.TryGetValue(category, out var keywords))
            {
                continue;
            }

            var confidence = 0m;
            foreach (var keyword in keywords)
            {
                var normalizedKeyword = keyword.ToLowerInvariant();
                if (tokens.Contains(normalizedKeyword))
                {
                    confidence = Math.Max(confidence, options1.ExactMatchConfidence);
                }
                else if (normalizedName.Contains(normalizedKeyword, StringComparison.Ordinal))
                {
                    confidence = Math.Max(confidence, options1.ContainsMatchConfidence);
                }
            }

            if (confidence > bestConfidence)
            {
                bestConfidence = confidence;
                bestCategory = category;
            }
        }

        if (bestCategory is null)
        {
            return new ClassificationSuggestion(null, 0m);
        }

        if (SamplePatternEligible.Contains(bestCategory.Value) && LooksDigitShaped(sampleMaskedValue))
        {
            bestConfidence = Math.Min(99m, bestConfidence + options1.SamplePatternBonus);
        }

        return new ClassificationSuggestion(bestCategory, bestConfidence);
    }

    private static bool LooksDigitShaped(string? maskedSample)
    {
        if (string.IsNullOrWhiteSpace(maskedSample))
        {
            return false;
        }

        var visibleChars = maskedSample.Replace("*", string.Empty);
        return visibleChars.Length > 0 && DigitShapedVisibleChars.IsMatch(visibleChars);
    }
}
