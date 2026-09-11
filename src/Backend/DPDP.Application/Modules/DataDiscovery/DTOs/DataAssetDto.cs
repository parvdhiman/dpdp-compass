namespace DPDP.Application.Modules.DataDiscovery.DTOs;

public sealed record DataAssetSummaryDto(
    Guid Id, Guid DataSourceId, string DataSourceName, string? DatabaseName, string? SchemaName,
    string AssetName, string AssetType, long? EstimatedRowCount, int ElementCount,
    int PersonalDataElementCount, DateTimeOffset? LastDiscoveredAt);

public sealed record DataAssetDetailDto(
    Guid Id, Guid DataSourceId, string DataSourceName, string? DatabaseName, string? SchemaName,
    string AssetName, string AssetType, string? FilePath, long? EstimatedRowCount,
    IReadOnlyList<string> Indexes, DateTimeOffset? LastDiscoveredAt,
    IReadOnlyList<DataElementDto> Elements);
