using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Compliance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.DeleteControlMapping;

public sealed record DeleteControlMappingCommand(Guid Id) : IRequest;

public sealed class DeleteControlMappingCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<DeleteControlMappingCommand>
{
    public async Task Handle(DeleteControlMappingCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance control library.");
        }

        var mapping = await db.ControlMappings.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ControlMapping), request.Id);

        db.ControlMappings.Remove(mapping);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.control_mapping_removed", nameof(ControlMapping), request.Id.ToString(), cancellationToken: cancellationToken);
    }
}
