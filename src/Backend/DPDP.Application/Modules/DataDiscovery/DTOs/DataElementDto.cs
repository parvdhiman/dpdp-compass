namespace DPDP.Application.Modules.DataDiscovery.DTOs;

public sealed record DataElementDto(
    Guid Id, Guid DataAssetId, string AssetName, string ColumnName, string DataType, bool IsNullable,
    int OrdinalPosition, string? SampleMaskedValue,
    string? ClassificationCategory, decimal? ClassificationConfidence, string? ClassificationSource,
    bool IsHumanCorrected, Guid? CorrectedByUserId, string? CorrectedByName, DateTimeOffset? CorrectedAt,
    DateTimeOffset? LastDiscoveredAt);

public sealed record ClassificationSummaryDto(
    int TotalElements, int UnclassifiedCount, int HumanCorrectedCount,
    IReadOnlyList<ClassificationCategoryCountDto> CategoryCounts);

public sealed record ClassificationCategoryCountDto(string Category, int Count);
