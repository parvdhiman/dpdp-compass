using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Commands.AssignUserRole;

public sealed record AssignUserRoleCommand(Guid UserId, Guid RoleId) : IRequest;

public sealed class AssignUserRoleCommandHandler(
    IAppDbContext db,
    ICurrentUserContext currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditLogger auditLogger)
    : IRequestHandler<AssignUserRoleCommand>
{
    public async Task Handle(AssignUserRoleCommand request, CancellationToken cancellationToken)
    {
        // The query filter already scopes this to the caller's tenant
        // unless the caller is a Super Administrator.
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Role), request.RoleId);

        if (role.Name == RoleNames.SuperAdministrator && !currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can assign the Super Administrator role.");
        }

        var alreadyAssigned = await db.UserRoles
            .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);
        if (alreadyAssigned)
        {
            throw new ConflictException("This role is already assigned to the user.");
        }

        db.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            OrganisationId = user.OrganisationId,
            AssignedAt = dateTimeProvider.UtcNow,
            AssignedBy = currentUser.UserId,
        });

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("user.role_assigned", nameof(User), user.Id.ToString(), newValue: new { role.Name }, cancellationToken: cancellationToken);
    }
}
