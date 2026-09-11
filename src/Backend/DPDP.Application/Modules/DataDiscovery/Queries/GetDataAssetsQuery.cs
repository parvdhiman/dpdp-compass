using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using DPDP.Domain.Modules.DataDiscovery;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.DataDiscovery.Queries;

public sealed record GetDataAssetsQuery(
    int Page = 1, int PageSize = 25, string? Search = null, Guid? DataSourceId = null, string? AssetType = null)
    : IRequest<PagedResult<DataAssetSummaryDto>>;

public sealed class GetDataAssetsQueryHandler(IAppDbContext db) : IRequestHandler<GetDataAssetsQuery, PagedResult<DataAssetSummaryDto>>
{
    public async Task<PagedResult<DataAssetSummaryDto>> Handle(GetDataAssetsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.DataAssets
            .AsNoTracking()
            .Include(a => a.DataSource)
            .Include(a => a.Elements)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(a => a.AssetName.ToUpper().Contains(term));
        }

        if (request.DataSourceId is { } dataSourceId)
        {
            query = query.Where(a => a.DataSourceId == dataSourceId);
        }

        if (!string.IsNullOrWhiteSpace(request.AssetType) && Enum.TryParse<DataAssetType>(request.AssetType, true, out var assetType))
        {
            query = query.Where(a => a.AssetType == assetType);
        }

        query = query.OrderByDescending(a => a.LastDiscoveredAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<DataAssetSummaryDto>(items.Select(DataDiscoveryMapper.ToSummaryDto).ToList(), page, pageSize, totalCount);
    }
}
