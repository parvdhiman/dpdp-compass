using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Identity.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Queries.GetPermissions;

public sealed record GetPermissionsQuery : IRequest<IReadOnlyList<PermissionDto>>;

public sealed class GetPermissionsQueryHandler(IAppDbContext db) : IRequestHandler<GetPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    public async Task<IReadOnlyList<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        return await db.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Module).ThenBy(p => p.Key)
            .Select(p => new PermissionDto(p.Id, p.Key, p.Description, p.Module))
            .ToListAsync(cancellationToken);
    }
}
