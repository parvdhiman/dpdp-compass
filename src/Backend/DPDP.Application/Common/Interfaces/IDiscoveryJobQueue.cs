namespace DPDP.Application.Common.Interfaces;

/// <summary>
/// In-process hand-off from a request (StartDiscoveryJobCommand) to the
/// background worker (DiscoveryJobBackgroundService) — see
/// docs/DATA_DISCOVERY.md section 5. A single-instance, in-memory queue is
/// the deliberate "first version" scope: it does not survive an API
/// process restart (a PENDING job left behind is simply never picked up
/// until something re-enqueues it), and does not distribute work across
/// multiple API instances. Swapping to a durable/distributed queue later
/// is a new implementation of this interface, not a redesign of any
/// command handler.
/// </summary>
public interface IDiscoveryJobQueue
{
    ValueTask EnqueueAsync(Guid jobId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken);
}
