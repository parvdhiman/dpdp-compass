using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Compliance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.DeleteEvidenceRequirement;

public sealed record DeleteEvidenceRequirementCommand(Guid Id) : IRequest;

public sealed class DeleteEvidenceRequirementCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteEvidenceRequirementCommand>
{
    public async Task Handle(DeleteEvidenceRequirementCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance control library.");
        }

        var evidence = await db.EvidenceRequirements.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(EvidenceRequirement), request.Id);

        evidence.IsDeleted = true;
        evidence.DeletedAt = dateTimeProvider.UtcNow;
        evidence.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.evidence_requirement_deleted", nameof(EvidenceRequirement), evidence.Id.ToString(), cancellationToken: cancellationToken);
    }
}
