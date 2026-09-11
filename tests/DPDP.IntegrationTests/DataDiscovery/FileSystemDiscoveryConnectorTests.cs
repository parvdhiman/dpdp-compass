using DPDP.Application.Modules.DataDiscovery.Connectors;
using DPDP.Domain.Modules.DataDiscovery;
using DPDP.Infrastructure.DataDiscovery.Connectors;
using Xunit;

namespace DPDP.IntegrationTests.DataDiscovery;

/// <summary>
/// Real file I/O against a throwaway temp directory — no database
/// involved, but placed here rather than DPDP.UnitTests because it needs
/// the Infrastructure project reference. See docs/DATA_DISCOVERY.md.
/// </summary>
public sealed class FileSystemDiscoveryConnectorTests : IDisposable
{
    private readonly FileSystemDiscoveryConnector _connector = new();
    private readonly DirectoryInfo _tempDir = Directory.CreateTempSubdirectory("dpdp-fs-connector-test-");

    private DataSourceConnectionInfo ConnectionInfo(int maxAssets = 500) =>
        new(null, null, null, null, null, _tempDir.FullName, null, SampleRowLimit: 5, MaxAssets: maxAssets);

    [Fact]
    public void Supported_type_is_file_system() => Assert.Equal(DataSourceType.FILE_SYSTEM, _connector.SupportedType);

    [Fact]
    public async Task Test_connection_succeeds_when_the_root_path_exists()
    {
        var result = await _connector.TestConnectionAsync(ConnectionInfo(), CancellationToken.None);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Test_connection_fails_when_the_root_path_does_not_exist()
    {
        var missing = new DataSourceConnectionInfo(null, null, null, null, null, Path.Combine(_tempDir.FullName, "does-not-exist"), null, 5, 500);
        var result = await _connector.TestConnectionAsync(missing, CancellationToken.None);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Discovers_a_tsv_file_with_masked_samples()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir.FullName, "data.tsv"), "id\temail\n1\tuser@example.com\n2\tother@example.com\n");

        var result = await _connector.DiscoverAsync(ConnectionInfo(), CancellationToken.None);

        var asset = Assert.Single(result.Assets);
        Assert.Equal(DataAssetType.FILE, asset.AssetType);
        Assert.Equal(2, asset.EstimatedRowCount);
        Assert.Equal(2, asset.Columns.Count);

        var emailColumn = asset.Columns.Single(c => c.ColumnName == "email");
        Assert.NotNull(emailColumn.SampleMaskedValue);
        Assert.DoesNotContain("user@example.com", emailColumn.SampleMaskedValue);
    }

    [Fact]
    public async Task A_non_delimited_file_becomes_one_opaque_content_element()
    {
        await File.WriteAllBytesAsync(Path.Combine(_tempDir.FullName, "report.pdf"), "%PDF-1.4"u8.ToArray());

        var result = await _connector.DiscoverAsync(ConnectionInfo(), CancellationToken.None);

        var asset = Assert.Single(result.Assets);
        var column = Assert.Single(asset.Columns);
        Assert.Equal("content", column.ColumnName);
        Assert.Null(column.SampleMaskedValue);
    }

    [Fact]
    public async Task Respects_the_max_assets_cap()
    {
        for (var i = 0; i < 5; i++)
        {
            await File.WriteAllTextAsync(Path.Combine(_tempDir.FullName, $"file{i}.txt"), "content");
        }

        var result = await _connector.DiscoverAsync(ConnectionInfo(maxAssets: 2), CancellationToken.None);

        Assert.Equal(2, result.Assets.Count);
    }

    [Fact]
    public async Task An_empty_directory_discovers_no_assets()
    {
        var result = await _connector.DiscoverAsync(ConnectionInfo(), CancellationToken.None);
        Assert.Empty(result.Assets);
    }

    public void Dispose() => _tempDir.Delete(recursive: true);
}
