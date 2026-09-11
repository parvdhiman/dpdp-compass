using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Compliance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.ActivateFrameworkVersion;

/// <summary>Marks one version as the current one for its Framework, unsetting any other IsCurrent version of the same Framework.</summary>
public sealed record ActivateFrameworkVersionCommand(Guid Id) : IRequest;

public sealed class ActivateFrameworkVersionCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<ActivateFrameworkVersionCommand>
{
    public async Task Handle(ActivateFrameworkVersionCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance framework library.");
        }

        var version = await db.FrameworkVersions.FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FrameworkVersion), request.Id);

        var otherCurrentVersions = await db.FrameworkVersions
            .Where(v => v.FrameworkId == version.FrameworkId && v.IsCurrent && v.Id != version.Id)
            .ToListAsync(cancellationToken);
        foreach (var other in otherCurrentVersions)
        {
            other.IsCurrent = false;
        }

        version.IsCurrent = true;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.framework_version_activated", nameof(FrameworkVersion), version.Id.ToString(), cancellationToken: cancellationToken);
    }
}
