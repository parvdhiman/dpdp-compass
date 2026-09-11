using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.DataDiscovery.Commands;

public sealed record DeleteDataSourceCommand(Guid Id) : IRequest;

public sealed class DeleteDataSourceCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteDataSourceCommand>
{
    public async Task Handle(DeleteDataSourceCommand request, CancellationToken cancellationToken)
    {
        var dataSource = await DataSourceLoader.LoadAsync(db, request.Id, cancellationToken);

        var hasActiveJob = await db.DiscoveryJobs.AnyAsync(
            j => j.DataSourceId == dataSource.Id && (j.Status == DiscoveryJobStatus.PENDING || j.Status == DiscoveryJobStatus.RUNNING),
            cancellationToken);
        if (hasActiveJob)
        {
            throw new ConflictException("Cannot remove a data source with a pending or running discovery job. Cancel it first.");
        }

        var now = dateTimeProvider.UtcNow;
        dataSource.IsDeleted = true;
        dataSource.DeletedAt = now;
        dataSource.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datadiscovery.datasource_deleted", nameof(DataSource), dataSource.Id.ToString(), cancellationToken: cancellationToken);
    }
}
