using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Organisations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Commands.DeleteOrganisationLocation;

public sealed record DeleteOrganisationLocationCommand(Guid LocationId) : IRequest;

public sealed class DeleteOrganisationLocationCommandHandler(
    IAppDbContext db,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserContext currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<DeleteOrganisationLocationCommand>
{
    public async Task Handle(DeleteOrganisationLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await db.OrganisationLocations
            .FirstOrDefaultAsync(l => l.Id == request.LocationId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganisationLocation), request.LocationId);

        location.IsDeleted = true;
        location.DeletedAt = dateTimeProvider.UtcNow;
        location.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("organisation.location_removed", nameof(OrganisationLocation), location.Id.ToString(), cancellationToken: cancellationToken);
    }
}
