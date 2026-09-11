namespace DPDP.Application.Modules.DataDiscovery.DTOs;

public sealed record DiscoveryJobDto(
    Guid Id, Guid DataSourceId, string DataSourceName, string Status,
    Guid TriggeredByUserId, string TriggeredByName,
    DateTimeOffset? StartedAt, DateTimeOffset? CompletedAt, string? ErrorMessage,
    int AssetsDiscoveredCount, int ElementsDiscoveredCount, DateTimeOffset CreatedAt);

public sealed record DiscoveryResultDto(
    Guid Id, Guid DataAssetId, string AssetName, long? RowCountAtScan, int ColumnsDiscovered, DateTimeOffset ScannedAt);

public sealed record DiscoveryJobDetailDto(
    Guid Id, Guid DataSourceId, string DataSourceName, string Status,
    Guid TriggeredByUserId, string TriggeredByName,
    DateTimeOffset? StartedAt, DateTimeOffset? CompletedAt, string? ErrorMessage,
    int AssetsDiscoveredCount, int ElementsDiscoveredCount, DateTimeOffset CreatedAt,
    IReadOnlyList<DiscoveryResultDto> Results);
