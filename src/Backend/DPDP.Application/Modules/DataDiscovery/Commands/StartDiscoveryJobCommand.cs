using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MediatR;

namespace DPDP.Application.Modules.DataDiscovery.Commands;

/// <summary>
/// Only ever creates a PENDING job and hands it to IDiscoveryJobQueue —
/// the actual scan runs entirely inside DiscoveryJobBackgroundService, off
/// the request thread. See docs/DATA_DISCOVERY.md section 5.
/// </summary>
public sealed record StartDiscoveryJobCommand(Guid DataSourceId) : IRequest<DiscoveryJobDto>;

public sealed class StartDiscoveryJobCommandHandler(
    IAppDbContext db, ICurrentUserContext currentUser, IOptions<DiscoveryOptions> discoveryOptions,
    IDiscoveryJobQueue jobQueue, IAuditLogger auditLogger)
    : IRequestHandler<StartDiscoveryJobCommand, DiscoveryJobDto>
{
    public async Task<DiscoveryJobDto> Handle(StartDiscoveryJobCommand request, CancellationToken cancellationToken)
    {
        var dataSource = await DataSourceLoader.LoadAsync(db, request.DataSourceId, cancellationToken);
        if (!dataSource.IsActive)
        {
            throw new ConflictException("Cannot start a discovery job against an inactive data source.");
        }

        var concurrentJobCount = await db.DiscoveryJobs.CountAsync(
            j => j.OrganisationId == dataSource.OrganisationId && (j.Status == DiscoveryJobStatus.PENDING || j.Status == DiscoveryJobStatus.RUNNING),
            cancellationToken);
        if (concurrentJobCount >= discoveryOptions.Value.MaxConcurrentJobsPerOrganisation)
        {
            throw new ConflictException($"This organisation already has {concurrentJobCount} discovery job(s) pending or running — the limit is {discoveryOptions.Value.MaxConcurrentJobsPerOrganisation}.");
        }

        var job = new DiscoveryJob
        {
            OrganisationId = dataSource.OrganisationId,
            DataSourceId = dataSource.Id,
            DataSource = dataSource,
            Status = DiscoveryJobStatus.PENDING,
            TriggeredByUserId = currentUser.UserId!.Value,
        };

        db.DiscoveryJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "datadiscovery.job_started", nameof(DiscoveryJob), job.Id.ToString(),
            newValue: new { dataSource.Name }, cancellationToken: cancellationToken);

        await jobQueue.EnqueueAsync(job.Id, cancellationToken);

        job.TriggeredByUser = await db.Users.FirstAsync(u => u.Id == currentUser.UserId, cancellationToken);
        return DataDiscoveryMapper.ToDto(job);
    }
}
