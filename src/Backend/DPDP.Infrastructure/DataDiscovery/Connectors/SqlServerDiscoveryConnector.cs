using DPDP.Application.Modules.DataDiscovery.Connectors;
using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.Data.SqlClient;

namespace DPDP.Infrastructure.DataDiscovery.Connectors;

/// <summary>
/// Reads only sys.* catalog views plus a bounded TOP-N sample per table
/// for masking — never a bulk export. ApplicationIntent=ReadOnly is set
/// as a best-effort hint (it is not a hard write-block on a primary
/// replica, unlike Postgres's default_transaction_read_only). See
/// docs/DATA_DISCOVERY.md sections 1 and 3. No live SQL Server exists in
/// this codebase's dev/test environment, so this connector is covered by
/// unit tests against its identifier-quoting/query-shape helpers rather
/// than a live integration test — see the completion report.
/// </summary>
public sealed class SqlServerDiscoveryConnector : IDiscoveryConnector
{
    public DataSourceType SupportedType => DataSourceType.SQLSERVER;

    public async Task<ConnectionTestResult> TestConnectionAsync(DataSourceConnectionInfo connectionInfo, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(BuildConnectionString(connectionInfo));
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
        await using var connection = new SqlConnection(BuildConnectionString(connectionInfo));
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

    private static string BuildConnectionString(DataSourceConnectionInfo connectionInfo) => new SqlConnectionStringBuilder
    {
        DataSource = connectionInfo.Port is { } port ? $"{connectionInfo.Host},{port}" : connectionInfo.Host,
        InitialCatalog = connectionInfo.DatabaseName,
        UserID = connectionInfo.Username,
        Password = connectionInfo.Password,
        ConnectTimeout = 10,
        CommandTimeout = 30,
        ApplicationIntent = ApplicationIntent.ReadOnly,
        TrustServerCertificate = true,
    }.ConnectionString;

    private static async Task<List<(string Schema, string Name, DataAssetType AssetType)>> ListTablesAsync(
        SqlConnection connection, string? schemaFilter, int maxAssets, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (@maxAssets) schema_name, table_name, table_type FROM (
                SELECT s.name AS schema_name, t.name AS table_name, 'TABLE' AS table_type
                FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
                UNION ALL
                SELECT s.name, v.name, 'VIEW'
                FROM sys.views v JOIN sys.schemas s ON s.schema_id = v.schema_id
            ) AS all_objects
            WHERE (@schemaFilter IS NULL OR schema_name = @schemaFilter)
            ORDER BY schema_name, table_name
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@maxAssets", maxAssets);
        command.Parameters.AddWithValue("@schemaFilter", (object?)schemaFilter ?? DBNull.Value);

        var results = new List<(string, string, DataAssetType)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var assetType = reader.GetString(2) == "VIEW" ? DataAssetType.VIEW : DataAssetType.TABLE;
            results.Add((reader.GetString(0), reader.GetString(1), assetType));
        }

        return results;
    }

    private static async Task<List<(string Name, string DataType, bool IsNullable, int OrdinalPosition)>> ListColumnsAsync(
        SqlConnection connection, string schema, string table, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT c.name, ty.name, c.is_nullable, c.column_id
            FROM sys.columns c
            JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            JOIN sys.tables t ON t.object_id = c.object_id
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE s.name = @schema AND t.name = @table
            ORDER BY c.column_id
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@schema", schema);
        command.Parameters.AddWithValue("@table", table);

        var results = new List<(string, string, bool, int)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add((reader.GetString(0), reader.GetString(1), reader.GetBoolean(2), reader.GetInt32(3)));
        }

        return results;
    }

    private static async Task<long?> EstimateRowCountAsync(SqlConnection connection, string schema, string table, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT SUM(p.rows)
            FROM sys.partitions p
            JOIN sys.tables t ON t.object_id = p.object_id
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE s.name = @schema AND t.name = @table AND p.index_id IN (0, 1)
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@schema", schema);
        command.Parameters.AddWithValue("@table", table);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is DBNull or null ? null : Convert.ToInt64(result);
    }

    private static async Task<List<string>> ListIndexesAsync(SqlConnection connection, string schema, string table, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT i.name
            FROM sys.indexes i
            JOIN sys.tables t ON t.object_id = i.object_id
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE s.name = @schema AND t.name = @table AND i.name IS NOT NULL
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@schema", schema);
        command.Parameters.AddWithValue("@table", table);

        var results = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(reader.GetString(0));
        }

        return results;
    }

    private static async Task<Dictionary<string, string?>> SampleColumnsAsync(
        SqlConnection connection, string schema, string table, IReadOnlyList<string> columnNames, int sampleRowLimit, CancellationToken cancellationToken)
    {
        var samples = columnNames.ToDictionary(c => c, _ => (string?)null);
        if (columnNames.Count == 0)
        {
            return samples;
        }

        var columnList = string.Join(", ", columnNames.Select(QuoteIdentifier));
        var sql = $"SELECT TOP (@limit) {columnList} FROM {QuoteIdentifier(schema)}.{QuoteIdentifier(table)}";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@limit", sampleRowLimit);

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

    private static string QuoteIdentifier(string identifier) => $"[{identifier.Replace("]", "]]")}]";
}
