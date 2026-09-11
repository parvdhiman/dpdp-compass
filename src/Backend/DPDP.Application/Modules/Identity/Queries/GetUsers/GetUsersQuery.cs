using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Identity.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Queries.GetUsers;

public sealed record GetUsersQuery(int Page = 1, int PageSize = 25, string? Search = null)
    : IRequest<PagedResult<UserDto>>;

/// <summary>
/// Relies entirely on DpdpDbContext's global query filter for tenant
/// scoping — this handler never adds its own OrganisationId predicate, by
/// design, so there is exactly one place tenant isolation can break. See
/// docs/ARCHITECTURE.md section 4 and the Module 2 tenant-isolation tests.
/// </summary>
public sealed class GetUsersQueryHandler(IAppDbContext db) : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.Users
            .AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderBy(u => u.FullName)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(u => u.NormalizedEmail.Contains(term) || u.FullName.ToUpper().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = users.Select(u => new UserDto(
                u.Id, u.OrganisationId, u.Email, u.FullName, u.PhoneNumber,
                u.IsActive, u.MustChangePassword, u.LastLoginAt, u.CreatedAt,
                u.UserRoles.Select(ur => new RoleSummaryDto(ur.RoleId, ur.Role.Name)).ToList()))
            .ToList();

        return new PagedResult<UserDto>(items, page, pageSize, totalCount);
    }
}
