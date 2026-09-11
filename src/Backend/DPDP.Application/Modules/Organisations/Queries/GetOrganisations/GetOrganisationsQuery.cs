using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Organisations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Queries.GetOrganisations;

/// <summary>
/// Cross-tenant listing — Super Administrator only (the query filter on
/// Organisation only excludes soft-deleted rows, not other tenants', so
/// this handler enforces the boundary itself rather than relying on the
/// filter). Every other organisation-scoped query only ever sees the
/// caller's own tenant via the filter, never a list of tenants.
/// </summary>
public sealed record GetOrganisationsQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    string? Status = null,
    string SortBy = "name",
    bool SortDescending = false) : IRequest<PagedResult<OrganisationSummaryDto>>;

public sealed class GetOrganisationsQueryHandler(IAppDbContext db, ICurrentUserContext currentUser)
    : IRequestHandler<GetOrganisationsQuery, PagedResult<OrganisationSummaryDto>>
{
    public async Task<PagedResult<OrganisationSummaryDto>> Handle(GetOrganisationsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can list every organisation.");
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.Organisations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(o => o.Name.ToUpper().Contains(term) || (o.LegalName != null && o.LegalName.ToUpper().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<Domain.Modules.Organisations.OrganisationStatus>(request.Status, true, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        query = (request.SortBy.ToLowerInvariant(), request.SortDescending) switch
        {
            ("createdat", false) => query.OrderBy(o => o.CreatedAt),
            ("createdat", true) => query.OrderByDescending(o => o.CreatedAt),
            (_, true) => query.OrderByDescending(o => o.Name),
            _ => query.OrderBy(o => o.Name),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrganisationSummaryDto(o.Id, o.Name, o.LegalName, o.Status.ToString(), o.Industry, o.Size == null ? null : o.Size.ToString(), o.Country, o.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrganisationSummaryDto>(items, page, pageSize, totalCount);
    }
}
