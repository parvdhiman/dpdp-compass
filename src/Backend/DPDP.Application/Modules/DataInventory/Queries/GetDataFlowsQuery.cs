using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataInventory.Commands;
using DPDP.Application.Modules.DataInventory.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.DataInventory.Queries;

public sealed record GetDataFlowsQuery(int Page = 1, int PageSize = 25, string? Search = null, Guid? ProcessingActivityId = null, bool? CrossBorderOnly = null)
    : IRequest<PagedResult<DataFlowDto>>;

public sealed class GetDataFlowsQueryHandler(IAppDbContext db) : IRequestHandler<GetDataFlowsQuery, PagedResult<DataFlowDto>>
{
    public async Task<PagedResult<DataFlowDto>> Handle(GetDataFlowsQuery request, CancellationToken cancellationToken)
    {
        var query = db.DataFlows
            .AsNoTracking()
            .Include(f => f.ProcessingActivity)
            .Include(f => f.DataCategory)
            .Include(f => f.FromItSystem)
            .Include(f => f.FromDataCollectionSource)
            .Include(f => f.ToItSystem)
            .Include(f => f.ToProcessor)
            .Include(f => f.ToRecipient)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(f => f.Name.ToUpper().Contains(term));
        }

        if (request.ProcessingActivityId is { } activityId) query = query.Where(f => f.ProcessingActivityId == activityId);
        if (request.CrossBorderOnly == true) query = query.Where(f => f.IsCrossBorder);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        query = query.OrderByDescending(f => f.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<DataFlowDto>(items.Select(DataInventoryMapper.ToDto).ToList(), page, pageSize, totalCount);
    }
}

public sealed record GetDataFlowByIdQuery(Guid Id) : IRequest<DataFlowDto>;

public sealed class GetDataFlowByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetDataFlowByIdQuery, DataFlowDto>
{
    public async Task<DataFlowDto> Handle(GetDataFlowByIdQuery request, CancellationToken cancellationToken) =>
        DataInventoryMapper.ToDto(await DataFlowLoader.LoadForDetailAsync(db, request.Id, cancellationToken));
}
