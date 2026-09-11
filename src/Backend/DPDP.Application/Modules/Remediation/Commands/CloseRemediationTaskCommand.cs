using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Remediation.DTOs;
using DPDP.Domain.Modules.Remediation;
using MediatR;

namespace DPDP.Application.Modules.Remediation.Commands;

public sealed record CloseRemediationTaskCommand(Guid Id) : IRequest<RemediationTaskDetailDto>;

public sealed class CloseRemediationTaskCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<CloseRemediationTaskCommand, RemediationTaskDetailDto>
{
    public async Task<RemediationTaskDetailDto> Handle(CloseRemediationTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await RemediationTaskLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!RemediationStatusTransitions.CanTransition(task.Status, RemediationStatus.CLOSED))
        {
            throw new ConflictException($"Cannot close a task in {task.Status} status — it must be VERIFIED first.");
        }

        task.Status = RemediationStatus.CLOSED;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("remediation.task_closed", nameof(RemediationTask), task.Id.ToString(), cancellationToken: cancellationToken);

        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);
        return RemediationMapper.ToDetailDto(task, today);
    }
}
