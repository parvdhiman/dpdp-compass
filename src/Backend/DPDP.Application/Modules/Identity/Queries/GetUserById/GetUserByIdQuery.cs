using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Identity.DTOs;
using DPDP.Domain.Modules.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;

public sealed class GetUserByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // A user in another tenant is excluded by the global query filter,
        // so this correctly returns 404 rather than leaking existence via 403.
        var user = await db.Users
            .AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        return new UserDto(
            user.Id, user.OrganisationId, user.Email, user.FullName, user.PhoneNumber,
            user.IsActive, user.MustChangePassword, user.LastLoginAt, user.CreatedAt,
            user.UserRoles.Select(ur => new RoleSummaryDto(ur.RoleId, ur.Role.Name)).ToList());
    }
}
