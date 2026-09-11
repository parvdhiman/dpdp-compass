namespace DPDP.Application.Modules.DataDiscovery.Connectors;

public sealed record DiscoveryScanResult(IReadOnlyList<DiscoveredAsset> Assets);

public sealed record ConnectionTestResult(bool Succeeded, string? ErrorMessage);
