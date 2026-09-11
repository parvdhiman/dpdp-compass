using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Commands.RevokeUserRole;

public sealed record RevokeUserRoleCommand(Guid UserId, Guid RoleId) : IRequest;

public sealed class RevokeUserRoleCommandHandler(
    IAppDbContext db,
    IAuditLogger auditLogger)
    : IRequestHandler<RevokeUserRoleCommand>
{
    public async Task Handle(RevokeUserRoleCommand request, CancellationToken cancellationToken)
    {
        // Confirms the user is visible to the caller's tenant before
        // touching the assignment — belt-and-suspenders alongside the
        // query filter.
        var userExists = await db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        var assignment = await db.UserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken)
            ?? throw new NotFoundException("UserRole assignment", $"{request.UserId}/{request.RoleId}");

        db.UserRoles.Remove(assignment);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("user.role_revoked", nameof(User), request.UserId.ToString(), oldValue: new { assignment.Role.Name }, cancellationToken: cancellationToken);
    }
}
