using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Compliance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.RetireControl;

/// <summary>Retiring never deletes anything — a retired control (and its questions/evidence) stays visible to admins and to any historical assessment that referenced it, just excluded from organisation-facing listings.</summary>
public sealed record RetireControlCommand(Guid Id) : IRequest;

public sealed class RetireControlCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<RetireControlCommand>
{
    public async Task Handle(RetireControlCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance control library.");
        }

        var control = await db.Controls.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Control), request.Id);

        if (control.Status == ControlStatus.RETIRED)
        {
            throw new ConflictException("This control is already retired.");
        }

        control.Status = ControlStatus.RETIRED;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.control_retired", nameof(Control), control.Id.ToString(), cancellationToken: cancellationToken);
    }
}
