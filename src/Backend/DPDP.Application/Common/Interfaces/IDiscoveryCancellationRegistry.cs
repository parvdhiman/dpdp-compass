namespace DPDP.Application.Common.Interfaces;

/// <summary>
/// Lets CancelDiscoveryJobCommand (a request-scoped handler) reach an
/// in-flight job being processed by DiscoveryJobBackgroundService (a
/// singleton). The persisted DiscoveryJob.CancellationRequested flag is
/// the fallback the worker polls between assets regardless — this
/// registry only makes cancellation faster, not correct, since the flag
/// alone is enough for the job to eventually stop. See
/// docs/DATA_DISCOVERY.md section 5.
/// </summary>
public interface IDiscoveryCancellationRegistry
{
    IDisposable Register(Guid jobId, CancellationTokenSource cancellationTokenSource);

    void RequestCancellation(Guid jobId);
}
