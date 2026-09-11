using DPDP.Application.Modules.DataDiscovery.Connectors;
using DPDP.Domain.Modules.DataDiscovery;

namespace DPDP.Infrastructure.DataDiscovery.Connectors;

/// <summary>
/// Walks a local directory tree. Deliberately shallow "first version"
/// scope: CSV/TSV files get real column-level discovery (header row +ARD
/// a few sample rows for masking); every other file type is recorded as
/// one opaque "content" element with no attempt to parse its bytes — see
/// docs/DATA_DISCOVERY.md section 1. Never follows symlinks/reparse
/// points, and re-validates every enumerated path stays under RootPath as
/// defense in depth, the same precedent as FileSystemObjectStorageService
/// (Module 7).
/// </summary>
public sealed class FileSystemDiscoveryConnector : IDiscoveryConnector
{
    private const long MaxBytesToCountLines = 5 * 1024 * 1024;
    private const int SampleDataLinesToRead = 20;

    public DataSourceType SupportedType => DataSourceType.FILE_SYSTEM;

    public Task<ConnectionTestResult> TestConnectionAsync(DataSourceConnectionInfo connectionInfo, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connectionInfo.RootPath))
        {
            return Task.FromResult(new ConnectionTestResult(false, "No root path configured."));
        }

        var root = Path.GetFullPath(connectionInfo.RootPath);
        return Task.FromResult(Directory.Exists(root)
            ? new ConnectionTestResult(true, null)
            : new ConnectionTestResult(false, "The configured root path does not exist or is not accessible."));
    }

    public Task<DiscoveryScanResult> DiscoverAsync(DataSourceConnectionInfo connectionInfo, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connectionInfo.RootPath))
        {
            throw new InvalidOperationException("No root path configured for this file system data source.");
        }

        var root = Path.GetFullPath(connectionInfo.RootPath);
        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException("The configured root path does not exist or is not accessible.");
        }

        var enumerationOptions = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            AttributesToSkip = FileAttributes.ReparsePoint | FileAttributes.System | FileAttributes.Hidden,
            IgnoreInaccessible = true,
        };

        var assets = new List<DiscoveredAsset>();
        foreach (var filePath in Directory.EnumerateFiles(root, "*", enumerationOptions))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (assets.Count >= connectionInfo.MaxAssets)
            {
                break;
            }

            var fullPath = Path.GetFullPath(filePath);
            if (!fullPath.StartsWith(root, StringComparison.Ordinal))
            {
                continue;
            }

            var relativePath = Path.GetRelativePath(root, fullPath).Replace('\\', '/');
            var extension = Path.GetExtension(fullPath).ToLowerInvariant();

            assets.Add(extension is ".csv" or ".tsv"
                ? BuildDelimitedFileAsset(fullPath, relativePath, extension)
                : BuildOpaqueFileAsset(fullPath, relativePath, extension));
        }

        return Task.FromResult(new DiscoveryScanResult(assets));
    }

    private static DiscoveredAsset BuildOpaqueFileAsset(string fullPath, string relativePath, string extension)
    {
        var element = new DiscoveredColumn("content", string.IsNullOrEmpty(extension) ? "unknown" : extension.TrimStart('.'), true, 0, null);
        return new DiscoveredAsset(null, null, relativePath, DataAssetType.FILE, relativePath, null, [], [element]);
    }

    private static DiscoveredAsset BuildDelimitedFileAsset(string fullPath, string relativePath, string extension)
    {
        var delimiter = extension == ".tsv" ? '\t' : ',';
        using var reader = new StreamReader(fullPath);

        var headerLine = reader.ReadLine();
        var headers = string.IsNullOrEmpty(headerLine) ? [] : headerLine.Split(delimiter);

        var sampleRows = new List<string[]>();
        for (var i = 0; i < SampleDataLinesToRead && !reader.EndOfStream; i++)
        {
            var line = reader.ReadLine();
            if (line is not null)
            {
                sampleRows.Add(line.Split(delimiter));
            }
        }

        var columns = new List<DiscoveredColumn>();
        for (var i = 0; i < headers.Length; i++)
        {
            var columnIndex = i;
            var sampleValue = sampleRows
                .Select(row => columnIndex < row.Length ? row[columnIndex] : null)
                .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

            columns.Add(new DiscoveredColumn(headers[i].Trim(), "string", true, i, SampleMasker.Mask(sampleValue)));
        }

        var fileInfo = new FileInfo(fullPath);
        long? rowCount = fileInfo.Length <= MaxBytesToCountLines ? CountDataLines(fullPath) : null;

        return new DiscoveredAsset(null, null, relativePath, DataAssetType.FILE, relativePath, rowCount, [], columns);
    }

    private static long CountDataLines(string fullPath)
    {
        using var reader = new StreamReader(fullPath);
        reader.ReadLine(); // header
        long count = 0;
        while (reader.ReadLine() is not null)
        {
            count++;
        }

        return count;
    }
}
