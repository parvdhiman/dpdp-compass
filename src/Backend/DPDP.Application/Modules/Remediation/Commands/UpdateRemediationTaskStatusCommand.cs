using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Remediation.DTOs;
using DPDP.Domain.Modules.Remediation;
using FluentValidation;
using MediatR;

namespace DPDP.Application.Modules.Remediation.Commands;

/// <summary>OPEN/IN_PROGRESS/PENDING_VERIFICATION only — VERIFIED and CLOSED go through their own dedicated commands.</summary>
public sealed record UpdateRemediationTaskStatusCommand(Guid Id, string Status) : IRequest<RemediationTaskDetailDto>;

public sealed class UpdateRemediationTaskStatusCommandValidator : AbstractValidator<UpdateRemediationTaskStatusCommand>
{
    public UpdateRemediationTaskStatusCommandValidator()
    {
        RuleFor(x => x.Status)
            .Must(v => Enum.TryParse<RemediationStatus>(v, out var s) && s is RemediationStatus.OPEN or RemediationStatus.IN_PROGRESS or RemediationStatus.PENDING_VERIFICATION)
            .WithMessage("status must be one of: OPEN, IN_PROGRESS, PENDING_VERIFICATION (use verify/close for the others)");
    }
}

public sealed class UpdateRemediationTaskStatusCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<UpdateRemediationTaskStatusCommand, RemediationTaskDetailDto>
{
    public async Task<RemediationTaskDetailDto> Handle(UpdateRemediationTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await RemediationTaskLoader.LoadForDetailAsync(db, request.Id, cancellationToken);
        var newStatus = Enum.Parse<RemediationStatus>(request.Status);

        if (!RemediationStatusTransitions.CanTransition(task.Status, newStatus))
        {
            throw new ConflictException($"Cannot move a remediation task from {task.Status} to {newStatus}.");
        }

        task.Status = newStatus;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("remediation.task_status_changed", nameof(RemediationTask), task.Id.ToString(), newValue: new { Status = newStatus.ToString() }, cancellationToken: cancellationToken);

        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);
        return RemediationMapper.ToDetailDto(task, today);
    }
}
