namespace DPDP.Application.Modules.DataDiscovery.Connectors;

/// <summary>
/// The decrypted connection coordinates for one scan, assembled just-in-time
/// by the command/background-service handler from a DataSource row plus
/// IConnectionSecretProtector.Unprotect — never persisted, never logged,
/// and scoped to the lifetime of a single DiscoverAsync/TestConnectionAsync
/// call. See docs/DATA_DISCOVERY.md section 2.
/// </summary>
public sealed record DataSourceConnectionInfo(
    string? Host,
    int? Port,
    string? DatabaseName,
    string? Username,
    string? Password,
    string? RootPath,
    string? SchemaFilter,
    int SampleRowLimit,
    int MaxAssets);
