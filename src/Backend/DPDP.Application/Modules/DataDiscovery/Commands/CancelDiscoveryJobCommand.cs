using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.DataDiscovery.Commands;

internal static class DiscoveryJobLoader
{
    public static async Task<DiscoveryJob> LoadForDetailAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.DiscoveryJobs
            .Include(j => j.DataSource)
            .Include(j => j.TriggeredByUser)
            .Include(j => j.Results).ThenInclude(r => r.DataAsset)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(DiscoveryJob), id);
}

/// <summary>
/// If the job is still PENDING (never dequeued), this alone is enough —
/// DiscoveryJobBackgroundService checks status before running a dequeued
/// job and simply skips one it finds already CANCELLED. If it is RUNNING,
/// this also asks IDiscoveryCancellationRegistry to signal the in-flight
/// connector immediately; CancellationRequested is the fallback the
/// worker polls between assets either way. See docs/DATA_DISCOVERY.md
/// section 5.
/// </summary>
public sealed record CancelDiscoveryJobCommand(Guid Id) : IRequest<DiscoveryJobDto>;

public sealed class CancelDiscoveryJobCommandHandler(
    IAppDbContext db, IDateTimeProvider dateTimeProvider, IDiscoveryCancellationRegistry cancellationRegistry, IAuditLogger auditLogger)
    : IRequestHandler<CancelDiscoveryJobCommand, DiscoveryJobDto>
{
    public async Task<DiscoveryJobDto> Handle(CancelDiscoveryJobCommand request, CancellationToken cancellationToken)
    {
        var job = await DiscoveryJobLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (DiscoveryJobStatusTransitions.IsTerminal(job.Status))
        {
            throw new ConflictException($"Cannot cancel a job that is already {job.Status}.");
        }

        job.CancellationRequested = true;
        if (job.Status == DiscoveryJobStatus.PENDING)
        {
            job.Status = DiscoveryJobStatus.CANCELLED;
            job.CompletedAt = dateTimeProvider.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        cancellationRegistry.RequestCancellation(job.Id);

        await auditLogger.LogAsync("datadiscovery.job_cancel_requested", nameof(DiscoveryJob), job.Id.ToString(), cancellationToken: cancellationToken);

        return DataDiscoveryMapper.ToDto(job);
    }
}
