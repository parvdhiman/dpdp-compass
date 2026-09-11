using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Domain.Modules.DataInventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.DataInventory.Queries;

public sealed record GetProcessingActivitiesQuery(
    int Page = 1, int PageSize = 25, string? Search = null, string? Status = null,
    Guid? OwnerUserId = null, Guid? DataCategoryId = null, Guid? ItSystemId = null)
    : IRequest<PagedResult<ProcessingActivitySummaryDto>>;

public sealed class GetProcessingActivitiesQueryHandler(IAppDbContext db) : IRequestHandler<GetProcessingActivitiesQuery, PagedResult<ProcessingActivitySummaryDto>>
{
    public async Task<PagedResult<ProcessingActivitySummaryDto>> Handle(GetProcessingActivitiesQuery request, CancellationToken cancellationToken)
    {
        var query = BuildFilteredQuery(db, request);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        query = query.OrderByDescending(a => a.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<ProcessingActivitySummaryDto>(items.Select(DataInventoryMapper.ToSummaryDto).ToList(), page, pageSize, totalCount);
    }

    internal static IQueryable<ProcessingActivity> BuildFilteredQuery(IAppDbContext db, GetProcessingActivitiesQuery request)
    {
        var query = db.ProcessingActivities
            .AsNoTracking()
            .Include(a => a.Owner)
            .Include(a => a.RetentionPolicy)
            .Include(a => a.DataCategories)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(a => a.Name.ToUpper().Contains(term) || a.Purpose.ToUpper().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ProcessingActivityStatus>(request.Status, true, out var status))
        {
            query = query.Where(a => a.Status == status);
        }

        if (request.OwnerUserId is { } ownerId) query = query.Where(a => a.OwnerUserId == ownerId);
        if (request.DataCategoryId is { } categoryId) query = query.Where(a => a.DataCategories.Any(c => c.Id == categoryId));
        if (request.ItSystemId is { } systemId) query = query.Where(a => a.ItSystems.Any(s => s.Id == systemId));

        return query;
    }
}
