using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Identity.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Queries.GetRoles;

public sealed record GetRolesQuery : IRequest<IReadOnlyList<RoleDto>>;

public sealed class GetRolesQueryHandler(IAppDbContext db) : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleDto>>
{
    public async Task<IReadOnlyList<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await db.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return roles.Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.Description,
                r.IsSystemRole,
                r.RolePermissions
                    .Select(rp => new PermissionDto(rp.Permission.Id, rp.Permission.Key, rp.Permission.Description, rp.Permission.Module))
                    .OrderBy(p => p.Key)
                    .ToList()))
            .ToList();
    }
}
