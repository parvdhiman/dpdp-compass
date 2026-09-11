using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Organisations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Queries.GetDepartments;

public sealed record GetDepartmentsQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    Guid? BusinessUnitId = null,
    bool? IsActive = null,
    string SortBy = "name",
    bool SortDescending = false) : IRequest<PagedResult<DepartmentDto>>;

/// <summary>Relies entirely on the global query filter for tenant scoping — see docs/ARCHITECTURE.md section 4.</summary>
public sealed class GetDepartmentsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetDepartmentsQuery, PagedResult<DepartmentDto>>
{
    public async Task<PagedResult<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.Departments.AsNoTracking().Include(d => d.BusinessUnit).AsQueryable();

        if (request.BusinessUnitId is { } businessUnitId)
        {
            query = query.Where(d => d.BusinessUnitId == businessUnitId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(d => d.Name.ToUpper().Contains(term) || (d.Description != null && d.Description.ToUpper().Contains(term)));
        }

        if (request.IsActive is { } isActive)
        {
            query = query.Where(d => d.IsActive == isActive);
        }

        query = (request.SortBy.ToLowerInvariant(), request.SortDescending) switch
        {
            ("createdat", false) => query.OrderBy(d => d.CreatedAt),
            ("createdat", true) => query.OrderByDescending(d => d.CreatedAt),
            (_, true) => query.OrderByDescending(d => d.Name),
            _ => query.OrderBy(d => d.Name),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var departments = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = departments
            .Select(d => new DepartmentDto(
                d.Id, d.OrganisationId, d.BusinessUnitId, d.BusinessUnit.Name,
                d.Name, d.Description, OrganisationMapper.ToDto(d.Head), d.IsActive, d.CreatedAt))
            .ToList();

        return new PagedResult<DepartmentDto>(items, page, pageSize, totalCount);
    }
}
