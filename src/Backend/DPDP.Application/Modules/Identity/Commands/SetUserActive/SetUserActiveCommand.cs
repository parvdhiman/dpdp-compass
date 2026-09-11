using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Identity.Commands.SetUserActive;

public sealed record SetUserActiveCommand(Guid UserId, bool IsActive) : IRequest;

public sealed class SetUserActiveCommandHandler(
    IAppDbContext db,
    ICurrentUserContext currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<SetUserActiveCommand>
{
    public async Task Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == currentUser.UserId && !request.IsActive)
        {
            throw new ConflictException("You cannot deactivate your own account.");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        user.IsActive = request.IsActive;

        if (!request.IsActive)
        {
            // Deactivation ends every active session immediately.
            var activeTokens = await db.RefreshTokens
                .IgnoreQueryFilters()
                .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
                .ToListAsync(cancellationToken);
            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTimeOffset.UtcNow;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            request.IsActive ? "user.activated" : "user.deactivated",
            nameof(User),
            user.Id.ToString(),
            cancellationToken: cancellationToken);
    }
}
