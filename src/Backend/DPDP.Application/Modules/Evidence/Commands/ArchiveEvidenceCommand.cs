using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Evidence.DTOs;
using DPDP.Domain.Modules.Evidence;
using MediatR;

namespace DPDP.Application.Modules.Evidence.Commands;

public sealed record ArchiveEvidenceCommand(Guid Id) : IRequest<EvidenceDetailDto>;

public sealed class ArchiveEvidenceCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<ArchiveEvidenceCommand, EvidenceDetailDto>
{
    public async Task<EvidenceDetailDto> Handle(ArchiveEvidenceCommand request, CancellationToken cancellationToken)
    {
        var evidence = await EvidenceLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!EvidenceStatusTransitions.CanTransition(evidence.Status, EvidenceStatus.ARCHIVED))
        {
            throw new ConflictException($"Cannot archive evidence in {evidence.Status} status.");
        }

        var now = dateTimeProvider.UtcNow;
        evidence.Status = EvidenceStatus.ARCHIVED;
        evidence.ArchivedAt = now;
        evidence.ArchivedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("evidence.archived", nameof(EvidenceItem), evidence.Id.ToString(), cancellationToken: cancellationToken);

        return EvidenceMapper.ToDetailDto(evidence, DateOnly.FromDateTime(now.UtcDateTime));
    }
}
