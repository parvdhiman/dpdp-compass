using DPDP.Application.Modules.DataDiscovery.Connectors;
using DPDP.Domain.Modules.DataDiscovery;
using Npgsql;

namespace DPDP.Infrastructure.DataDiscovery.Connectors;

/// <summary>
/// Reads only catalog metadata (information_schema/pg_catalog) plus a
/// bounded LIMIT-ed sample per table for masking — never a bulk export.
/// The connection is opened with default_transaction_read_only=on as
/// defense in depth: even a bug in this connector could not write to the
/// customer's database. See docs/DATA_DISCOVERY.md sections 1 and 3.
/// </summary>
public sealed class PostgresDiscoveryConnector : IDiscoveryConnector
{
    public DataSourceType SupportedType => DataSourceType.POSTGRESQL;

    public async Task<ConnectionTestResult> TestConnectionAsync(DataSourceConnectionInfo connectionInfo, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(BuildConnectionString(connectionInfo));
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
        await using var connection = new NpgsqlConnection(BuildConnectionString(connectionInfo));
        await connection.OpenAsync(cancellationToken);

        var tables = await ListTablesAsync(connection, connectionInfo.SchemaFilter, connectionInfo.MaxAssets, cancellationToken);
        var assets = new List<DiscoveredAsset>();

        foreach (var (schema, name, assetType) in tables)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var columns = await ListColumnsAsync(connection, schema, name, cancellationToken);
            var rowCount = await EstimateRowCountAsync(connection, schema, name, cancellationToken);
            var indexes = await ListIndexesAsync(connection, schema, name, cancellationToken);
            var samples = await SampleColumnsAsync(connection, schema, name, columns.Select(c => c.Name).ToList(), connectionInfo.SampleRowLimit, cancellationToken);

            var discoveredColumns = columns
                .Select(c => new DiscoveredColumn(c.Name, c.DataType, c.IsNullable, c.OrdinalPosition, SampleMasker.Mask(samples.GetValueOrDefault(c.Name))))
                .ToList();

            assets.Add(new DiscoveredAsset(connectionInfo.DatabaseName, schema, name, assetType, null, rowCount, indexes, discoveredColumns));
        }

        return new DiscoveryScanResult(assets);
    }

    private static string BuildConnectionString(DataSourceConnectionInfo connectionInfo) => new NpgsqlConnectionStringBuilder
    {
        Host = connectionInfo.Host,
        Port = connectionInfo.Port ?? 5432,
        Database = connectionInfo.DatabaseName,
        Username = connectionInfo.Username,
        Password = connectionInfo.Password,
        Timeout = 10,
        CommandTimeout = 30,
        Options = "-c default_transaction_read_only=on",
    }.ConnectionString;

    private static async Task<List<(string Schema, string Name, DataAssetType AssetType)>> ListTablesAsync(
        NpgsqlConnection connection, string? schemaFilter, int maxAssets, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT table_schema, table_name, table_type
            FROM information_schema.tables
            WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
              AND (@schemaFilter::text IS NULL OR table_schema = @schemaFilter)
            ORDER BY table_schema, table_name
            LIMIT @maxAssets
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("schemaFilter", (object?)schemaFilter ?? DBNull.Value);
        command.Parameters.AddWithValue("maxAssets", maxAssets);

        var results = new List<(string, string, DataAssetType)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var tableType = reader.GetString(2) == "VIEW" ? DataAssetType.VIEW : DataAssetType.TABLE;
            results.Add((reader.GetString(0), reader.GetString(1), tableType));
        }

        return results;
    }

    private static async Task<List<(string Name, string DataType, bool IsNullable, int OrdinalPosition)>> ListColumnsAsync(
        NpgsqlConnection connection, string schema, string table, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT column_name, data_type, is_nullable, ordinal_position
            FROM information_schema.columns
            WHERE table_schema = @schema AND table_name = @table
            ORDER BY ordinal_position
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("schema", schema);
        command.Parameters.AddWithValue("table", table);

        var results = new List<(string, string, bool, int)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2) == "YES", reader.GetInt32(3)));
        }

        return results;
    }

    private static async Task<long?> EstimateRowCountAsync(NpgsqlConnection connection, string schema, string table, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT c.reltuples::bigint
            FROM pg_class c
            JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = @schema AND c.relname = @table
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("schema", schema);
        command.Parameters.AddWithValue("table", table);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        var estimate = result as long?;
        return estimate is null or < 0 ? null : estimate;
    }

    private static async Task<List<string>> ListIndexesAsync(NpgsqlConnection connection, string schema, string table, CancellationToken cancellationToken)
    {
        const string sql = "SELECT indexname FROM pg_indexes WHERE schemaname = @schema AND tablename = @table";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("schema", schema);
        command.Parameters.AddWithValue("table", table);

        var results = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(reader.GetString(0));
        }

        return results;
    }

    /// <summary>
    /// One bounded LIMIT query fetches a handful of rows across every
    /// column at once (cheaper than one query per column); only the first
    /// non-null value seen per column is kept, and only long enough to
    /// mask it — the fetched rows themselves are discarded the moment this
    /// method returns. See docs/DATA_DISCOVERY.md section 3.
    /// </summary>
    private static async Task<Dictionary<string, string?>> SampleColumnsAsync(
        NpgsqlConnection connection, string schema, string table, IReadOnlyList<string> columnNames, int sampleRowLimit, CancellationToken cancellationToken)
    {
        var samples = columnNames.ToDictionary(c => c, _ => (string?)null);
        if (columnNames.Count == 0)
        {
            return samples;
        }

        var columnList = string.Join(", ", columnNames.Select(QuoteIdentifier));
        var sql = $"SELECT {columnList} FROM {QuoteIdentifier(schema)}.{QuoteIdentifier(table)} LIMIT @limit";

        await using var command = new NpgsqlCommand(sql, connection);
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

    private static string QuoteIdentifier(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";
}
