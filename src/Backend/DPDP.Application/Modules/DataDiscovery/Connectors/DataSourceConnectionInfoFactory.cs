using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.DataDiscovery;

namespace DPDP.Application.Modules.DataDiscovery.Connectors;

/// <summary>
/// The one place a DataSource's encrypted secret is decrypted — both
/// TestDataSourceConnectionCommand and DiscoveryJobBackgroundService go
/// through this rather than calling IConnectionSecretProtector.Unprotect
/// themselves, so there is a single, reviewable call site for "plaintext
/// credential exists in memory" moments.
/// </summary>
internal static class DataSourceConnectionInfoFactory
{
    public static DataSourceConnectionInfo Build(DataSource dataSource, IConnectionSecretProtector secretProtector, DiscoveryOptions options) =>
        new(
            dataSource.Host,
            dataSource.Port,
            dataSource.DatabaseName,
            dataSource.Username,
            dataSource.EncryptedSecret is null ? null : secretProtector.Unprotect(dataSource.EncryptedSecret),
            dataSource.RootPath,
            dataSource.SchemaFilter,
            options.SampleRowLimit,
            options.MaxAssetsPerJob);
}
