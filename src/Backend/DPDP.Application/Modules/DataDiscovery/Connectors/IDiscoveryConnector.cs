using DPDP.Domain.Modules.DataDiscovery;

namespace DPDP.Application.Modules.DataDiscovery.Connectors;

/// <summary>
/// One implementation per DataSourceType (Postgres/MySQL/SqlServer/
/// FileSystem) — see docs/DATA_DISCOVERY.md section 1. Every
/// implementation must: read schema/metadata only through the target
/// engine's own catalog views (never SELECT * at scale), bound any sample
/// query with connectionInfo.SampleRowLimit, mask every sample value via
/// SampleMasker before returning it, and honor cancellationToken between
/// assets so a CancelDiscoveryJobCommand takes effect promptly on a
/// multi-table scan instead of only at the very end.
/// </summary>
public interface IDiscoveryConnector
{
    DataSourceType SupportedType { get; }

    Task<ConnectionTestResult> TestConnectionAsync(DataSourceConnectionInfo connectionInfo, CancellationToken cancellationToken);

    Task<DiscoveryScanResult> DiscoverAsync(DataSourceConnectionInfo connectionInfo, CancellationToken cancellationToken);
}
