using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Remediation.DTOs;
using DPDP.Domain.Modules.Remediation;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Remediation.Commands;

/// <summary>
/// "Verification" (Module 6 brief). Deliberately does not cascade to the
/// parent Finding's own status — verifying a remediation task only needs
/// remediation.manage, while closing a Finding needs findings.close; auto-
/// cascading would let a remediation-manage holder effectively close a
/// finding without that permission. See docs/ARCHITECTURE.md Module 6
/// section.
/// </summary>
public sealed record VerifyRemediationTaskCommand(Guid Id, string? VerificationNotes) : IRequest<RemediationTaskDetailDto>;

public sealed class VerifyRemediationTaskCommandValidator : AbstractValidator<VerifyRemediationTaskCommand>
{
    public VerifyRemediationTaskCommandValidator()
    {
        RuleFor(x => x.VerificationNotes).MaximumLength(2000);
    }
}

public sealed class VerifyRemediationTaskCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<VerifyRemediationTaskCommand, RemediationTaskDetailDto>
{
    public async Task<RemediationTaskDetailDto> Handle(VerifyRemediationTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await RemediationTaskLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!RemediationStatusTransitions.CanTransition(task.Status, RemediationStatus.VERIFIED))
        {
            throw new ConflictException($"Cannot verify a task in {task.Status} status — it must be pending verification.");
        }

        var verifier = await db.Users.FirstAsync(u => u.Id == currentUser.UserId!.Value, cancellationToken);
        var now = dateTimeProvider.UtcNow;

        task.Status = RemediationStatus.VERIFIED;
        task.VerifiedByUserId = verifier.Id;
        task.VerifiedByUser = verifier;
        task.VerifiedAt = now;
        task.VerificationNotes = request.VerificationNotes;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("remediation.task_verified", nameof(RemediationTask), task.Id.ToString(), cancellationToken: cancellationToken);

        var today = DateOnly.FromDateTime(now.UtcDateTime);
        return RemediationMapper.ToDetailDto(task, today);
    }
}
