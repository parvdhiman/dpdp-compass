using System.Text.Json;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using DPDP.Domain.Modules.DataDiscovery;

namespace DPDP.Application.Modules.DataDiscovery;

internal static class DataDiscoveryMapper
{
    public static DataSourceSummaryDto ToSummaryDto(DataSource source) => new(
        source.Id, source.Name, source.SourceType.ToString(), source.IsActive,
        source.LastTestedAt, source.LastTestSucceeded, source.CreatedAt);

    public static DataSourceDetailDto ToDetailDto(DataSource source) => new(
        source.Id, source.Name, source.Description, source.SourceType.ToString(),
        source.Host, source.Port, source.DatabaseName, source.Username, source.RootPath, source.SchemaFilter,
        source.IsActive, source.LastTestedAt, source.LastTestSucceeded, source.LastTestError,
        source.CreatedAt, source.UpdatedAt);

    public static DiscoveryJobDto ToDto(DiscoveryJob job) => new(
        job.Id, job.DataSourceId, job.DataSource?.Name ?? string.Empty, job.Status.ToString(),
        job.TriggeredByUserId, job.TriggeredByUser?.FullName ?? string.Empty,
        job.StartedAt, job.CompletedAt, job.ErrorMessage,
        job.AssetsDiscoveredCount, job.ElementsDiscoveredCount, job.CreatedAt);

    public static DiscoveryResultDto ToDto(DiscoveryResult result) => new(
        result.Id, result.DataAssetId, result.DataAsset?.AssetName ?? string.Empty,
        result.RowCountAtScan, result.ColumnsDiscovered, result.ScannedAt);

    public static DiscoveryJobDetailDto ToDetailDto(DiscoveryJob job) => new(
        job.Id, job.DataSourceId, job.DataSource?.Name ?? string.Empty, job.Status.ToString(),
        job.TriggeredByUserId, job.TriggeredByUser?.FullName ?? string.Empty,
        job.StartedAt, job.CompletedAt, job.ErrorMessage,
        job.AssetsDiscoveredCount, job.ElementsDiscoveredCount, job.CreatedAt,
        job.Results.OrderByDescending(r => r.ScannedAt).Select(ToDto).ToList());

    public static IReadOnlyList<string> ParseIndexes(string? indexesJson) =>
        string.IsNullOrWhiteSpace(indexesJson)
            ? []
            : JsonSerializer.Deserialize<List<string>>(indexesJson) ?? [];

    public static string SerializeIndexes(IReadOnlyList<string> indexes) => JsonSerializer.Serialize(indexes);

    public static DataAssetSummaryDto ToSummaryDto(DataAsset asset) => new(
        asset.Id, asset.DataSourceId, asset.DataSource?.Name ?? string.Empty, asset.DatabaseName, asset.SchemaName,
        asset.AssetName, asset.AssetType.ToString(), asset.EstimatedRowCount, asset.Elements.Count,
        asset.Elements.Count(e => e.ClassificationCategory != null), asset.LastDiscoveredAt);

    public static DataAssetDetailDto ToDetailDto(DataAsset asset) => new(
        asset.Id, asset.DataSourceId, asset.DataSource?.Name ?? string.Empty, asset.DatabaseName, asset.SchemaName,
        asset.AssetName, asset.AssetType.ToString(), asset.FilePath, asset.EstimatedRowCount,
        ParseIndexes(asset.IndexesJson), asset.LastDiscoveredAt,
        asset.Elements.OrderBy(e => e.OrdinalPosition).Select(ToDto).ToList());

    public static DataElementDto ToDto(DataElement element) => new(
        element.Id, element.DataAssetId, element.DataAsset?.AssetName ?? string.Empty,
        element.ColumnName, element.DataType, element.IsNullable, element.OrdinalPosition, element.SampleMaskedValue,
        element.ClassificationCategory?.ToString(), element.ClassificationConfidence, element.ClassificationSource?.ToString(),
        element.IsHumanCorrected, element.CorrectedByUserId, element.CorrectedByUser?.FullName, element.CorrectedAt,
        element.LastDiscoveredAt);
}
