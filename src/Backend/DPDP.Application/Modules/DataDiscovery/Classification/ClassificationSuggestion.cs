using DPDP.Domain.Modules.DataDiscovery;

namespace DPDP.Application.Modules.DataDiscovery.Classification;

/// <summary>A suggestion, never a determination — see docs/DATA_DISCOVERY.md section 4. Category is null when no keyword rule matched at all (the classifier found no evidence this column is personal data).</summary>
public sealed record ClassificationSuggestion(ClassificationCategory? Category, decimal Confidence);
