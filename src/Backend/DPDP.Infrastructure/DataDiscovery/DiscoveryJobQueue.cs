using System.Threading.Channels;
using DPDP.Application.Common.Interfaces;

namespace DPDP.Infrastructure.DataDiscovery;

/// <summary>The only implementation — in-process, in-memory. See docs/DATA_DISCOVERY.md section 5 for its documented limitations.</summary>
public sealed class DiscoveryJobQueue : IDiscoveryJobQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });

    public async ValueTask EnqueueAsync(Guid jobId, CancellationToken cancellationToken = default) =>
        await _channel.Writer.WriteAsync(jobId, cancellationToken);

    public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
