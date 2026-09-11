using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataDiscovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DPDP.Infrastructure.DataDiscovery;

/// <summary>
/// The sole consumer of IDiscoveryJobQueue. Processes jobs one at a time
/// (deliberate "first version" simplicity — see docs/DATA_DISCOVERY.md
/// section 5; a future version could run N workers reading the same
/// channel). For each job, bridges DB-flag-based cancellation
/// (DiscoveryJob.CancellationRequested, polled every few seconds from a
/// fresh scope) and in-process registry-based cancellation
/// (IDiscoveryCancellationRegistry, immediate) into one CancellationToken
/// that IDiscoveryJobProcessor/IDiscoveryConnector only need to check
/// normally.
/// </summary>
public sealed class DiscoveryJobBackgroundService(
    IDiscoveryJobQueue jobQueue,
    IDiscoveryCancellationRegistry cancellationRegistry,
    IServiceScopeFactory scopeFactory,
    ILogger<DiscoveryJobBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan CancellationPollInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var jobId in jobQueue.DequeueAllAsync(stoppingToken))
        {
            await ProcessOneJobAsync(jobId, stoppingToken);
        }
    }

    private async Task ProcessOneJobAsync(Guid jobId, CancellationToken stoppingToken)
    {
        using var jobCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        using var registration = cancellationRegistry.Register(jobId, jobCts);
        using var pollCts = new CancellationTokenSource();

        var pollTask = PollForCancellationRequestAsync(jobId, jobCts, pollCts.Token);

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<IDiscoveryJobProcessor>();
            await processor.ProcessAsync(jobId, jobCts.Token);
        }
        catch (Exception ex)
        {
            // IDiscoveryJobProcessor already turns every failure into a
            // FAILED job row — reaching here means something outside that
            // contract broke (e.g. resolving the scope itself), which is
            // worth a loud log but must never crash the whole worker loop.
            logger.LogError(ex, "Unexpected error while dispatching discovery job {JobId}.", jobId);
        }
        finally
        {
            await pollCts.CancelAsync();
            try
            {
                await pollTask;
            }
            catch (OperationCanceledException)
            {
                // Expected — this is how the poll loop is told to stop.
            }
        }
    }

    private async Task PollForCancellationRequestAsync(Guid jobId, CancellationTokenSource jobCts, CancellationToken pollToken)
    {
        try
        {
            while (!pollToken.IsCancellationRequested)
            {
                await Task.Delay(CancellationPollInterval, pollToken);

                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                // Outside any authenticated HTTP request — see the
                // IgnoreQueryFilters comment in DiscoveryJobProcessor for why
                // this is required here too.
                var cancellationRequested = await db.DiscoveryJobs
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(j => j.Id == jobId)
                    .Select(j => j.CancellationRequested)
                    .FirstOrDefaultAsync(pollToken);

                if (cancellationRequested)
                {
                    await jobCts.CancelAsync();
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown of the poll loop once the job finished.
        }
    }
}
