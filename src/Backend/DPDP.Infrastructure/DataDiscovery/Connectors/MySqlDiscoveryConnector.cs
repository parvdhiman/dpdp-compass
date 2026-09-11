using DPDP.Application.Modules.DataDiscovery.Connectors;
using DPDP.Domain.Modules.DataDiscovery;
using MySqlConnector;

namespace DPDP.Infrastructure.DataDiscovery.Connectors;

/// <summary>
/// Reads only information_schema metadata plus a bounded LIMIT-ed sample
/// per table for masking — never a bulk export. See
/// docs/DATA_DISCOVERY.md sections 1 and 3. No live MySQL server exists in
/// this codebase's dev/test environment, so this connector is covered by
/// unit tests against its identifier-quoting/query-shape helpers rather
/// than a live integration test — see the completion report.
/// </summary>
public sealed class MySqlDiscoveryConnector : IDiscoveryConnector
{
    public DataSourceType SupportedType => DataSourceType.MYSQL;

    public async Task<ConnectionTestResult> TestConnectionAsync(DataSourceConnectionInfo connectionInfo, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new MySqlConnection(BuildConnectionString(connectionInfo));
            await connection.OpenAsync(cancellationToken);
            return new ConnectionTestResult(true, null);
        }
        catch (Exception ex)
        {
            return new ConnectionTestResult(false, ex.Message);
        }
    }

    public async Task<DiscoveryScanResult> DiscoverAsync(DataSourceConnectionInfo connectionInfo, CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(BuildConnectionString(connectionInfo));
        await connection.OpenAsync(cancellationToken);
        await TrySetReadOnlyAsync(connection, cancellationToken);

        var tables = await ListTablesAsync(connection, connectionInfo.DatabaseName!, connectionInfo.MaxAssets, cancellationToken);
        var assets = new List<DiscoveredAsset>();

        foreach (var (name, assetType, estimatedRowCount) in tables)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var columns = await ListColumnsAsync(connection, connectionInfo.DatabaseName!, name, cancellationToken);
            var indexes = await ListIndexesAsync(connection, connectionInfo.DatabaseName!, name, cancellationToken);
            var samples = await SampleColumnsAsync(connection, connectionInfo.DatabaseName!, name, columns.Select(c => c.Name).ToList(), connectionInfo.SampleRowLimit, cancellationToken);

            var discoveredColumns = columns
                .Select(c => new DiscoveredColumn(c.Name, c.DataType, c.IsNullable, c.OrdinalPosition, SampleMasker.Mask(samples.GetValueOrDefault(c.Name))))
                .ToList();

            assets.Add(new DiscoveredAsset(connectionInfo.DatabaseName, null, name, assetType, null, estimatedRowCount, indexes, discoveredColumns));
        }

        return new DiscoveryScanResult(assets);
    }

    private static string BuildConnectionString(DataSourceConnectionInfo connectionInfo) => new MySqlConnectionStringBuilder
    {
        Server = connectionInfo.Host,
        Port = (uint)(connectionInfo.Port ?? 3306),
        Database = connectionInfo.DatabaseName,
        UserID = connectionInfo.Username,
        Password = connectionInfo.Password,
        ConnectionTimeout = 10,
        DefaultCommandTimeout = 30,
    }.ConnectionString;

    private static async Task TrySetReadOnlyAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            await using var command = new MySqlCommand("SET SESSION TRANSACTION READ ONLY", connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (MySqlException)
        {
            // Some hosted MySQL configurations deny this to the discovery
            // account — best-effort defense in depth, not a hard
            // requirement, so a denial doesn't block discovery.
        }
    }

    private static async Task<List<(string Name, DataAssetType AssetType, long? EstimatedRowCount)>> ListTablesAsync(
        MySqlConnection connection, string database, int maxAssets, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT table_name, table_type, table_rows
            FROM information_schema.tables
            WHERE table_schema = @database
            ORDER BY table_name
            LIMIT @maxAssets
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("database", database);
        command.Parameters.AddWithValue("maxAssets", maxAssets);

        var results = new List<(string, DataAssetType, long?)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var assetType = reader.GetString(1) == "VIEW" ? DataAssetType.VIEW : DataAssetType.TABLE;
            var rowCount = await reader.IsDBNullAsync(2, cancellationToken) ? null : (long?)Convert.ToInt64(reader.GetValue(2));
            results.Add((reader.GetString(0), assetType, rowCount));
        }

        return results;
    }

    private static async Task<List<(string Name, string DataType, bool IsNullable, int OrdinalPosition)>> ListColumnsAsync(
        MySqlConnection connection, string database, string table, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT column_name, data_type, is_nullable, ordinal_position
            FROM information_schema.columns
            WHERE table_schema = @database AND table_name = @table
            ORDER BY ordinal_position
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("database", database);
        command.Parameters.AddWithValue("table", table);

        var results = new List<(string, string, bool, int)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2) == "YES", Convert.ToInt32(reader.GetValue(3))));
        }

        return results;
    }

    private static async Task<List<string>> ListIndexesAsync(MySqlConnection connection, string database, string table, CancellationToken cancellationToken)
    {
        const string sql = "SELECT DISTINCT index_name FROM information_schema.statistics WHERE table_schema = @database AND table_name = @table";

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("database", database);
        command.Parameters.AddWithValue("table", table);

        var results = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(reader.GetString(0));
        }

        return results;
    }

    private static async Task<Dictionary<string, string?>> SampleColumnsAsync(
        MySqlConnection connection, string database, string table, IReadOnlyList<string> columnNames, int sampleRowLimit, CancellationToken cancellationToken)
    {
        var samples = columnNames.ToDictionary(c => c, _ => (string?)null);
        if (columnNames.Count == 0)
        {
            return samples;
        }

        var columnList = string.Join(", ", columnNames.Select(QuoteIdentifier));
        var sql = $"SELECT {columnList} FROM {QuoteIdentifier(database)}.{QuoteIdentifier(table)} LIMIT @limit";

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("limit", sampleRowLimit);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            for (var i = 0; i < columnNames.Count; i++)
            {
                if (samples[columnNames[i]] is not null || await reader.IsDBNullAsync(i, cancellationToken))
                {
                    continue;
                }

                samples[columnNames[i]] = reader.GetValue(i).ToString();
            }
        }

        return samples;
    }

    private static string QuoteIdentifier(string identifier) => $"`{identifier.Replace("`", "``")}`";
}
