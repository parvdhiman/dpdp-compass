namespace DPDP.Application.Modules.DataDiscovery;

/// <summary>
/// Runs one DiscoveryJob to completion: resolves the right connector,
/// upserts DataAsset/DataElement rows, classifies new/changed elements,
/// and updates job status. Called by DiscoveryJobBackgroundService inside
/// a fresh DI scope per job — see docs/DATA_DISCOVERY.md section 5. Lives
/// in the Application layer (not Infrastructure) because it is pure
/// orchestration over Application-visible abstractions
/// (IAppDbContext/IDiscoveryConnectorResolver/IDataClassificationStrategy)
/// — no hosting concern belongs here.
/// </summary>
public interface IDiscoveryJobProcessor
{
    Task ProcessAsync(Guid jobId, CancellationToken cancellationToken);
}
