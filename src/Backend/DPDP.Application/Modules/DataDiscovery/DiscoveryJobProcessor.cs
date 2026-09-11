using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataDiscovery.Classification;
using DPDP.Application.Modules.DataDiscovery.Connectors;
using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DPDP.Application.Modules.DataDiscovery;

public sealed class DiscoveryJobProcessor(
    IAppDbContext db,
    IConnectionSecretProtector secretProtector,
    IDiscoveryConnectorResolver connectorResolver,
    IDataClassificationStrategy classificationStrategy,
    IOptions<DiscoveryOptions> discoveryOptions,
    IDateTimeProvider dateTimeProvider,
    IAuditLogger auditLogger,
    ILogger<DiscoveryJobProcessor> logger)
    : IDiscoveryJobProcessor
{
    public async Task ProcessAsync(Guid jobId, CancellationToken cancellationToken)
    {
        // This runs in a DI scope created by DiscoveryJobBackgroundService,
        // not inside an authenticated HTTP request — ICurrentUserContext
        // resolves to "no organisation", so DpdpDbContext's tenant query
        // filters would silently exclude every row and make every job look
        // like it doesn't exist. IgnoreQueryFilters() here is the same
        // precedent as Identity's pre-auth queries (Login/Refresh/
        // ResetPassword) — see docs/ARCHITECTURE.md section 11 and
        // docs/DATA_DISCOVERY.md section 5. Safe: jobId was already
        // validated as belonging to a real organisation by
        // StartDiscoveryJobCommand under a real authenticated request; this
        // method only ever touches the one job/asset graph it was told to.
        var job = await db.DiscoveryJobs.IgnoreQueryFilters().Include(j => j.DataSource).FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
        if (job is null || job.Status != DiscoveryJobStatus.PENDING)
        {
            // Already cancelled while still queued, or otherwise no longer
            // runnable — nothing to do.
            return;
        }

        var now = dateTimeProvider.UtcNow;
        job.Status = DiscoveryJobStatus.RUNNING;
        job.StartedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var connectionInfo = DataSourceConnectionInfoFactory.Build(job.DataSource, secretProtector, discoveryOptions.Value);
            var connector = connectorResolver.Resolve(job.DataSource.SourceType);
            var scanResult = await connector.DiscoverAsync(connectionInfo, cancellationToken);

            foreach (var discoveredAsset in scanResult.Assets.Take(discoveryOptions.Value.MaxAssetsPerJob))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ApplyDiscoveredAssetAsync(job, discoveredAsset, cancellationToken);
            }

            job.Status = DiscoveryJobStatus.COMPLETED;
            job.CompletedAt = dateTimeProvider.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            await auditLogger.LogAsync(
                "datadiscovery.job_completed", nameof(DiscoveryJob), job.Id.ToString(),
                newValue: new { job.AssetsDiscoveredCount, job.ElementsDiscoveredCount }, cancellationToken: CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            job.Status = DiscoveryJobStatus.CANCELLED;
            job.CompletedAt = dateTimeProvider.UtcNow;
            await db.SaveChangesAsync(CancellationToken.None);
            await auditLogger.LogAsync("datadiscovery.job_cancelled", nameof(DiscoveryJob), job.Id.ToString(), cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Discovery job {JobId} against data source {DataSourceId} failed.", job.Id, job.DataSourceId);
            job.Status = DiscoveryJobStatus.FAILED;
            job.CompletedAt = dateTimeProvider.UtcNow;
            job.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            await db.SaveChangesAsync(CancellationToken.None);
            await auditLogger.LogAsync(
                "datadiscovery.job_failed", nameof(DiscoveryJob), job.Id.ToString(),
                newValue: new { job.ErrorMessage }, cancellationToken: CancellationToken.None);
        }
    }

    private async Task ApplyDiscoveredAssetAsync(DiscoveryJob job, DiscoveredAsset discoveredAsset, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;

        var asset = await db.DataAssets.IgnoreQueryFilters().Include(a => a.Elements).FirstOrDefaultAsync(
            a => a.DataSourceId == job.DataSourceId && a.SchemaName == discoveredAsset.SchemaName && a.AssetName == discoveredAsset.AssetName,
            cancellationToken);

        if (asset is null)
        {
            asset = new DataAsset
            {
                OrganisationId = job.OrganisationId,
                DataSourceId = job.DataSourceId,
                SchemaName = discoveredAsset.SchemaName,
                AssetName = discoveredAsset.AssetName,
            };
            db.DataAssets.Add(asset);
        }

        asset.DatabaseName = discoveredAsset.DatabaseName;
        asset.AssetType = discoveredAsset.AssetType;
        asset.FilePath = discoveredAsset.FilePath;
        asset.EstimatedRowCount = discoveredAsset.EstimatedRowCount;
        asset.IndexesJson = DataDiscoveryMapper.SerializeIndexes(discoveredAsset.Indexes);
        asset.LastDiscoveredAt = now;
        asset.LastDiscoveryJobId = job.Id;

        foreach (var discoveredColumn in discoveredAsset.Columns)
        {
            var element = asset.Elements.FirstOrDefault(e => e.ColumnName == discoveredColumn.ColumnName);
            if (element is null)
            {
                element = new DataElement
                {
                    OrganisationId = job.OrganisationId,
                    DataAsset = asset,
                    ColumnName = discoveredColumn.ColumnName,
                };
                asset.Elements.Add(element);
                db.DataElements.Add(element);
            }

            element.DataType = discoveredColumn.DataType;
            element.IsNullable = discoveredColumn.IsNullable;
            element.OrdinalPosition = discoveredColumn.OrdinalPosition;
            element.SampleMaskedValue = discoveredColumn.SampleMaskedValue;
            element.LastDiscoveredAt = now;

            // A human's classification survives every future re-scan of the
            // same column — only ever re-run the system classifier when no
            // human has weighed in.
            if (!element.IsHumanCorrected)
            {
                var suggestion = classificationStrategy.Classify(discoveredColumn.ColumnName, discoveredColumn.DataType, discoveredColumn.SampleMaskedValue);
                element.ClassificationCategory = suggestion.Category;
                element.ClassificationConfidence = suggestion.Category is null ? null : suggestion.Confidence;
                element.ClassificationSource = suggestion.Category is null ? null : ClassificationSource.SYSTEM;
            }
        }

        var result = new DiscoveryResult
        {
            OrganisationId = job.OrganisationId,
            DiscoveryJobId = job.Id,
            DataAsset = asset,
            RowCountAtScan = discoveredAsset.EstimatedRowCount,
            ColumnsDiscovered = discoveredAsset.Columns.Count,
            IndexesJson = asset.IndexesJson,
            ScannedAt = now,
        };
        db.DiscoveryResults.Add(result);

        job.AssetsDiscoveredCount++;
        job.ElementsDiscoveredCount += discoveredAsset.Columns.Count;

        await db.SaveChangesAsync(cancellationToken);
    }
}
