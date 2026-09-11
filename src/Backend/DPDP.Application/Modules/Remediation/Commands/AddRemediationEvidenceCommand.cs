using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Remediation.DTOs;
using DPDP.Domain.Modules.Remediation;
using FluentValidation;
using MediatR;

namespace DPDP.Application.Modules.Remediation.Commands;

/// <summary>Replaces the task's evidence list. Attaching evidence signals "ready to be checked," so a task still OPEN/IN_PROGRESS moves to PENDING_VERIFICATION automatically.</summary>
public sealed record AddRemediationEvidenceCommand(Guid Id, IReadOnlyList<EvidenceReferenceDto> Evidence) : IRequest<RemediationTaskDetailDto>;

public sealed class AddRemediationEvidenceCommandValidator : AbstractValidator<AddRemediationEvidenceCommand>
{
    public AddRemediationEvidenceCommandValidator()
    {
        RuleFor(x => x.Evidence).NotEmpty();
    }
}

public sealed class AddRemediationEvidenceCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<AddRemediationEvidenceCommand, RemediationTaskDetailDto>
{
    public async Task<RemediationTaskDetailDto> Handle(AddRemediationEvidenceCommand request, CancellationToken cancellationToken)
    {
        var task = await RemediationTaskLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        task.EvidenceJson = RemediationMapper.SerializeEvidence(request.Evidence);
        if (task.Status is RemediationStatus.OPEN or RemediationStatus.IN_PROGRESS)
        {
            task.Status = RemediationStatus.PENDING_VERIFICATION;
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("remediation.evidence_added", nameof(RemediationTask), task.Id.ToString(), newValue: new { Count = request.Evidence.Count }, cancellationToken: cancellationToken);

        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);
        return RemediationMapper.ToDetailDto(task, today);
    }
}
