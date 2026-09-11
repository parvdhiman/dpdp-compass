using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Commands.RevokeRolePermission;

public sealed record RevokeRolePermissionCommand(Guid RoleId, Guid PermissionId) : IRequest;

public sealed class RevokeRolePermissionCommandHandler(
    IAppDbContext db,
    ICurrentUserContext currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<RevokeRolePermissionCommand>
{
    public async Task Handle(RevokeRolePermissionCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can modify role permission templates.");
        }

        var assignment = await db.RolePermissions
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => rp.RoleId == request.RoleId && rp.PermissionId == request.PermissionId, cancellationToken)
            ?? throw new NotFoundException("RolePermission assignment", $"{request.RoleId}/{request.PermissionId}");

        db.RolePermissions.Remove(assignment);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("role.permission_revoked", nameof(Role), request.RoleId.ToString(), oldValue: new { assignment.Permission.Key }, cancellationToken: cancellationToken);
    }
}
