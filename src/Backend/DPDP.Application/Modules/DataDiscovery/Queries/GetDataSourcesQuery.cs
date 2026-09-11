using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using DPDP.Domain.Modules.DataDiscovery;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.DataDiscovery.Queries;

public sealed record GetDataSourcesQuery(
    int Page = 1, int PageSize = 25, string? Search = null, string? SourceType = null, bool? IsActive = null)
    : IRequest<PagedResult<DataSourceSummaryDto>>;

public sealed class GetDataSourcesQueryHandler(IAppDbContext db) : IRequestHandler<GetDataSourcesQuery, PagedResult<DataSourceSummaryDto>>
{
    public async Task<PagedResult<DataSourceSummaryDto>> Handle(GetDataSourcesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.DataSources.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(s => s.Name.ToUpper().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.SourceType) && Enum.TryParse<DataSourceType>(request.SourceType, true, out var sourceType))
        {
            query = query.Where(s => s.SourceType == sourceType);
        }

        if (request.IsActive is { } isActive)
        {
            query = query.Where(s => s.IsActive == isActive);
        }

        query = query.OrderByDescending(s => s.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<DataSourceSummaryDto>(items.Select(DataDiscoveryMapper.ToSummaryDto).ToList(), page, pageSize, totalCount);
    }
}
