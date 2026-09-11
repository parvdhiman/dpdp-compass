using DPDP.Domain.Modules.DataDiscovery;

namespace DPDP.Application.Modules.DataDiscovery.Connectors;

public sealed record DiscoveredAsset(
    string? DatabaseName,
    string? SchemaName,
    string AssetName,
    DataAssetType AssetType,
    string? FilePath,
    long? EstimatedRowCount,
    IReadOnlyList<string> Indexes,
    IReadOnlyList<DiscoveredColumn> Columns);
