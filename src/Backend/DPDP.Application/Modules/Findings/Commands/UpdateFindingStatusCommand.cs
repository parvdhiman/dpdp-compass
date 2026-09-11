using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Findings.DTOs;
using DPDP.Domain.Modules.Findings;
using FluentValidation;
using MediatR;

namespace DPDP.Application.Modules.Findings.Commands;

/// <summary>General progress transitions only — ASSIGNED/IN_PROGRESS/PENDING_VERIFICATION/RESOLVED. CLOSED and ACCEPTED_RISK go through their own dedicated commands (closing-authority actions) — see FindingStatusTransitions.</summary>
public sealed record UpdateFindingStatusCommand(Guid Id, string Status) : IRequest<FindingDetailDto>;

public sealed class UpdateFindingStatusCommandValidator : AbstractValidator<UpdateFindingStatusCommand>
{
    public UpdateFindingStatusCommandValidator()
    {
        RuleFor(x => x.Status)
            .Must(v => Enum.TryParse<FindingStatus>(v, out var s) && s is FindingStatus.ASSIGNED or FindingStatus.IN_PROGRESS or FindingStatus.PENDING_VERIFICATION or FindingStatus.RESOLVED)
            .WithMessage("status must be one of: ASSIGNED, IN_PROGRESS, PENDING_VERIFICATION, RESOLVED (use the close/accept-risk actions for the others)");
    }
}

public sealed class UpdateFindingStatusCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<UpdateFindingStatusCommand, FindingDetailDto>
{
    public async Task<FindingDetailDto> Handle(UpdateFindingStatusCommand request, CancellationToken cancellationToken)
    {
        var finding = await FindingLoader.LoadForDetailAsync(db, request.Id, cancellationToken);
        var newStatus = Enum.Parse<FindingStatus>(request.Status);

        if (!FindingStatusTransitions.CanTransition(finding.Status, newStatus))
        {
            throw new ConflictException($"Cannot move a finding from {finding.Status} to {newStatus}.");
        }

        var oldStatus = finding.Status;
        finding.Status = newStatus;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("findings.status_changed", nameof(Finding), finding.Id.ToString(), new { Status = oldStatus.ToString() }, new { Status = newStatus.ToString() }, cancellationToken);

        return FindingMapper.ToDetailDto(finding);
    }
}
