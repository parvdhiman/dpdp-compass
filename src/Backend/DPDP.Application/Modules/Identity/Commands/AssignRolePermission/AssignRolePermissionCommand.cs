using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Commands.AssignRolePermission;

/// <summary>
/// Roles are global, shared across every tenant — editing a role's
/// permission template is a system-wide change, so it requires Super
/// Administrator specifically, not merely the roles.manage permission
/// (which also covers org-scoped role *assignment* to users, a much
/// narrower blast radius). See the Module 2 completion report.
/// </summary>
public sealed record AssignRolePermissionCommand(Guid RoleId, Guid PermissionId) : IRequest;

public sealed class AssignRolePermissionCommandHandler(
    IAppDbContext db,
    ICurrentUserContext currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<AssignRolePermissionCommand>
{
    public async Task Handle(AssignRolePermissionCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can modify role permission templates.");
        }

        var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Role), request.RoleId);

        var permission = await db.Permissions.FirstOrDefaultAsync(p => p.Id == request.PermissionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Permission), request.PermissionId);

        var alreadyAssigned = await db.RolePermissions
            .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id, cancellationToken);
        if (alreadyAssigned)
        {
            throw new ConflictException("This permission is already assigned to the role.");
        }

        db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("role.permission_assigned", nameof(Role), role.Id.ToString(), newValue: new { permission.Key }, cancellationToken: cancellationToken);
    }
}
