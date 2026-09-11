using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using DPDP.Domain.Modules.DataDiscovery;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.DataDiscovery.Queries;

/// <summary>Backs both the "Data Elements" and "Classification" dashboard views.</summary>
public sealed record GetDataElementsQuery(
    int Page = 1, int PageSize = 25, Guid? DataAssetId = null, string? Category = null,
    bool? UnclassifiedOnly = null, bool? LowConfidenceOnly = null)
    : IRequest<PagedResult<DataElementDto>>;

public sealed class GetDataElementsQueryHandler(IAppDbContext db) : IRequestHandler<GetDataElementsQuery, PagedResult<DataElementDto>>
{
    private const decimal LowConfidenceThreshold = 60m;

    public async Task<PagedResult<DataElementDto>> Handle(GetDataElementsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.DataElements
            .AsNoTracking()
            .Include(e => e.DataAsset)
            .Include(e => e.CorrectedByUser)
            .AsQueryable();

        if (request.DataAssetId is { } dataAssetId)
        {
            query = query.Where(e => e.DataAssetId == dataAssetId);
        }

        if (!string.IsNullOrWhiteSpace(request.Category) && Enum.TryParse<ClassificationCategory>(request.Category, true, out var category))
        {
            query = query.Where(e => e.ClassificationCategory == category);
        }

        if (request.UnclassifiedOnly == true)
        {
            query = query.Where(e => e.ClassificationCategory == null);
        }

        if (request.LowConfidenceOnly == true)
        {
            query = query.Where(e => e.ClassificationCategory != null && e.ClassificationConfidence < LowConfidenceThreshold && !e.IsHumanCorrected);
        }

        query = query.OrderByDescending(e => e.LastDiscoveredAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<DataElementDto>(items.Select(DataDiscoveryMapper.ToDto).ToList(), page, pageSize, totalCount);
    }
}
