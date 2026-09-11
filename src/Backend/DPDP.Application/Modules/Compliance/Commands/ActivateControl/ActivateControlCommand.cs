using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Compliance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.ActivateControl;

public sealed record ActivateControlCommand(Guid Id) : IRequest;

public sealed class ActivateControlCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<ActivateControlCommand>
{
    public async Task Handle(ActivateControlCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance control library.");
        }

        var control = await db.Controls.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Control), request.Id);

        if (control.Status == ControlStatus.ACTIVE)
        {
            throw new ConflictException("This control is already active.");
        }

        control.Status = ControlStatus.ACTIVE;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.control_activated", nameof(Control), control.Id.ToString(), cancellationToken: cancellationToken);
    }
}
