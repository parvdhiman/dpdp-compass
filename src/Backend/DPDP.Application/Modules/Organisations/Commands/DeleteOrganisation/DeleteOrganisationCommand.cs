using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Organisations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Commands.DeleteOrganisation;

/// <summary>
/// Super Administrator only — deleting an entire tenant is the highest
/// blast-radius action in the system. Soft-delete only, and refuses if the
/// organisation still has any active (non-deleted) user, so a tenant can't
/// be silently orphaned by mistake.
/// </summary>
public sealed record DeleteOrganisationCommand(Guid OrganisationId) : IRequest;

public sealed class DeleteOrganisationCommandHandler(
    IAppDbContext db,
    ICurrentUserContext currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditLogger auditLogger)
    : IRequestHandler<DeleteOrganisationCommand>
{
    public async Task Handle(DeleteOrganisationCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can delete an organisation.");
        }

        var organisation = await db.Organisations
            .FirstOrDefaultAsync(o => o.Id == request.OrganisationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Organisation), request.OrganisationId);

        var hasActiveUsers = await db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.OrganisationId == request.OrganisationId && !u.IsDeleted, cancellationToken);
        if (hasActiveUsers)
        {
            throw new ConflictException("Cannot delete an organisation that still has active users.");
        }

        var now = dateTimeProvider.UtcNow;
        organisation.IsDeleted = true;
        organisation.DeletedAt = now;
        organisation.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("organisation.deleted", nameof(Organisation), organisation.Id.ToString(), cancellationToken: cancellationToken);
    }
}
