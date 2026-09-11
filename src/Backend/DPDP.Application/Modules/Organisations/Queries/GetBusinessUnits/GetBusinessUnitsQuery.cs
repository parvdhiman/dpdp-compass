using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Organisations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Queries.GetBusinessUnits;

public sealed record GetBusinessUnitsQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    bool? IsActive = null,
    string SortBy = "name",
    bool SortDescending = false) : IRequest<PagedResult<BusinessUnitDto>>;

/// <summary>Relies entirely on the global query filter for tenant scoping — see docs/ARCHITECTURE.md section 4.</summary>
public sealed class GetBusinessUnitsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetBusinessUnitsQuery, PagedResult<BusinessUnitDto>>
{
    public async Task<PagedResult<BusinessUnitDto>> Handle(GetBusinessUnitsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.BusinessUnits.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(b => b.Name.ToUpper().Contains(term) || (b.Description != null && b.Description.ToUpper().Contains(term)));
        }

        if (request.IsActive is { } isActive)
        {
            query = query.Where(b => b.IsActive == isActive);
        }

        query = (request.SortBy.ToLowerInvariant(), request.SortDescending) switch
        {
            ("createdat", false) => query.OrderBy(b => b.CreatedAt),
            ("createdat", true) => query.OrderByDescending(b => b.CreatedAt),
            (_, true) => query.OrderByDescending(b => b.Name),
            _ => query.OrderBy(b => b.Name),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var businessUnits = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new { b.Id, b.OrganisationId, b.Name, b.Description, b.Head, b.IsActive, b.CreatedAt, DepartmentCount = b.Departments.Count })
            .ToListAsync(cancellationToken);

        var items = businessUnits
            .Select(b => new BusinessUnitDto(b.Id, b.OrganisationId, b.Name, b.Description, OrganisationMapper.ToDto(b.Head), b.IsActive, b.DepartmentCount, b.CreatedAt))
            .ToList();

        return new PagedResult<BusinessUnitDto>(items, page, pageSize, totalCount);
    }
}
