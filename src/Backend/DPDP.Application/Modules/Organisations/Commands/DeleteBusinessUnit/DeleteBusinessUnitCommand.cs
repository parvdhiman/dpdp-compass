using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Organisations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Commands.DeleteBusinessUnit;

/// <summary>Refuses to delete a business unit that still has departments — same conservative pattern as DeleteOrganisationCommand.</summary>
public sealed record DeleteBusinessUnitCommand(Guid Id) : IRequest;

public sealed class DeleteBusinessUnitCommandHandler(
    IAppDbContext db,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserContext currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<DeleteBusinessUnitCommand>
{
    public async Task Handle(DeleteBusinessUnitCommand request, CancellationToken cancellationToken)
    {
        var businessUnit = await db.BusinessUnits
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(BusinessUnit), request.Id);

        var hasDepartments = await db.Departments.AnyAsync(d => d.BusinessUnitId == request.Id, cancellationToken);
        if (hasDepartments)
        {
            throw new ConflictException("Cannot delete a business unit that still has departments.");
        }

        businessUnit.IsDeleted = true;
        businessUnit.DeletedAt = dateTimeProvider.UtcNow;
        businessUnit.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("business_unit.deleted", nameof(BusinessUnit), businessUnit.Id.ToString(), cancellationToken: cancellationToken);
    }
}
