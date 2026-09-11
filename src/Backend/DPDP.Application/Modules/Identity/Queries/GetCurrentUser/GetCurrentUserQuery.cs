using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Identity.DTOs;
using DPDP.Domain.Modules.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<MeDto>;

public sealed class GetCurrentUserQueryHandler(IAppDbContext db, ICurrentUserContext currentUser)
    : IRequestHandler<GetCurrentUserQuery, MeDto>
{
    public async Task<MeDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), currentUser.UserId ?? Guid.Empty);

        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Key)
            .Distinct()
            .ToList();

        return new MeDto(user.Id, user.OrganisationId, user.Email, user.FullName, currentUser.IsSuperAdministrator, roleNames, permissions);
    }
}
