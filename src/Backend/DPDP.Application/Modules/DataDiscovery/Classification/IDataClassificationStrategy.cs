namespace DPDP.Application.Modules.DataDiscovery.Classification;

/// <summary>
/// Swappable, same "interface + options, resolved via DI" shape as
/// IComplianceScoringStrategy (Module 5) and IRiskScoringStrategy (Module
/// 6) — see docs/DATA_DISCOVERY.md section 4.
/// </summary>
public interface IDataClassificationStrategy
{
    ClassificationSuggestion Classify(string columnName, string dataType, string? sampleMaskedValue);
}
