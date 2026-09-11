using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Findings.DTOs;
using DPDP.Domain.Modules.Findings;
using MediatR;

namespace DPDP.Application.Modules.Findings.Commands;

public sealed record CloseFindingCommand(Guid Id) : IRequest<FindingDetailDto>;

public sealed class CloseFindingCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<CloseFindingCommand, FindingDetailDto>
{
    public async Task<FindingDetailDto> Handle(CloseFindingCommand request, CancellationToken cancellationToken)
    {
        var finding = await FindingLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!FindingStatusTransitions.CanTransition(finding.Status, FindingStatus.CLOSED))
        {
            throw new ConflictException($"Cannot close a finding in {finding.Status} status — it must be RESOLVED first.");
        }

        finding.Status = FindingStatus.CLOSED;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("findings.closed", nameof(Finding), finding.Id.ToString(), cancellationToken: cancellationToken);

        return FindingMapper.ToDetailDto(finding);
    }
}
