using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using DPDP.Domain.Modules.DataDiscovery;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.DataDiscovery.Queries;

public sealed record GetDiscoveryJobsQuery(
    int Page = 1, int PageSize = 25, Guid? DataSourceId = null, string? Status = null)
    : IRequest<PagedResult<DiscoveryJobDto>>;

public sealed class GetDiscoveryJobsQueryHandler(IAppDbContext db) : IRequestHandler<GetDiscoveryJobsQuery, PagedResult<DiscoveryJobDto>>
{
    public async Task<PagedResult<DiscoveryJobDto>> Handle(GetDiscoveryJobsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.DiscoveryJobs
            .AsNoTracking()
            .Include(j => j.DataSource)
            .Include(j => j.TriggeredByUser)
            .AsQueryable();

        if (request.DataSourceId is { } dataSourceId)
        {
            query = query.Where(j => j.DataSourceId == dataSourceId);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<DiscoveryJobStatus>(request.Status, true, out var status))
        {
            query = query.Where(j => j.Status == status);
        }

        query = query.OrderByDescending(j => j.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<DiscoveryJobDto>(items.Select(DataDiscoveryMapper.ToDto).ToList(), page, pageSize, totalCount);
    }
}
