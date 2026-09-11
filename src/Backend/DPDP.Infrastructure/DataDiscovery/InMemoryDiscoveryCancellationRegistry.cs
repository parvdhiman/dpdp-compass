using System.Collections.Concurrent;
using DPDP.Application.Common.Interfaces;

namespace DPDP.Infrastructure.DataDiscovery;

public sealed class InMemoryDiscoveryCancellationRegistry : IDiscoveryCancellationRegistry
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _tokenSources = new();

    public IDisposable Register(Guid jobId, CancellationTokenSource cancellationTokenSource)
    {
        _tokenSources[jobId] = cancellationTokenSource;
        return new Unregisterer(this, jobId);
    }

    public void RequestCancellation(Guid jobId)
    {
        if (_tokenSources.TryGetValue(jobId, out var cts))
        {
            try
            {
                cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // The job already finished and disposed its token source
                // between the lookup and the cancel — nothing left to do.
            }
        }
    }

    private sealed class Unregisterer(InMemoryDiscoveryCancellationRegistry registry, Guid jobId) : IDisposable
    {
        public void Dispose() => registry._tokenSources.TryRemove(jobId, out _);
    }
}
