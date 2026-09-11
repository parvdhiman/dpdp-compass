using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.DataDiscovery;
using DPDP.Domain.Modules.DataInventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.DataInventory.Queries;

/// <summary>Backs the Data Inventory dashboard's search/filter — see docs/DATA_INVENTORY.md.</summary>
public sealed record GetDataInventoryItemsQuery(
    int Page = 1, int PageSize = 25, string? Search = null, Guid? DataCategoryId = null,
    Guid? ItSystemId = null, Guid? ProcessorId = null, string? Classification = null, string? RiskLevel = null)
    : IRequest<PagedResult<DataInventoryItemDto>>;

public sealed class GetDataInventoryItemsQueryHandler(IAppDbContext db) : IRequestHandler<GetDataInventoryItemsQuery, PagedResult<DataInventoryItemDto>>
{
    public async Task<PagedResult<DataInventoryItemDto>> Handle(GetDataInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var query = BuildFilteredQuery(db, request);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        query = query.OrderByDescending(i => i.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<DataInventoryItemDto>(items.Select(DataInventoryMapper.ToDto).ToList(), page, pageSize, totalCount);
    }

    internal static IQueryable<DataInventoryItem> BuildFilteredQuery(IAppDbContext db, GetDataInventoryItemsQuery request)
    {
        var query = db.DataInventoryItems
            .AsNoTracking()
            .Include(i => i.DataCategory)
            .Include(i => i.DataCollectionSource)
            .Include(i => i.ItSystem)
            .Include(i => i.Owner)
            .Include(i => i.RetentionPolicy)
            .Include(i => i.Processor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(i => i.DataElementName.ToUpper().Contains(term) || (i.Purpose != null && i.Purpose.ToUpper().Contains(term)));
        }

        if (request.DataCategoryId is { } categoryId) query = query.Where(i => i.DataCategoryId == categoryId);
        if (request.ItSystemId is { } systemId) query = query.Where(i => i.ItSystemId == systemId);
        if (request.ProcessorId is { } processorId) query = query.Where(i => i.ProcessorId == processorId);

        if (!string.IsNullOrWhiteSpace(request.Classification) && Enum.TryParse<ClassificationCategory>(request.Classification, true, out var classification))
        {
            query = query.Where(i => i.Classification == classification);
        }

        if (!string.IsNullOrWhiteSpace(request.RiskLevel) && Enum.TryParse<RiskLevel>(request.RiskLevel, true, out var riskLevel))
        {
            query = query.Where(i => i.RiskLevel == riskLevel);
        }

        return query;
    }
}
