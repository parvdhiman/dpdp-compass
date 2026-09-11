using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Findings;
using DPDP.Application.Modules.Remediation.DTOs;
using DPDP.Domain.Modules.Findings;
using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Remediation;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Remediation.Commands;

internal static class RemediationTaskLoader
{
    public static async Task<RemediationTask> LoadForDetailAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.RemediationTasks
            .Include(t => t.Finding)
            .Include(t => t.Owner)
            .Include(t => t.VerifiedByUser)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(RemediationTask), id);
}

public sealed record CreateRemediationTaskCommand(Guid FindingId, string Title, string? Description, Guid? OwnerUserId, DateOnly? DueDate)
    : IRequest<RemediationTaskDetailDto>;

public sealed class CreateRemediationTaskCommandValidator : AbstractValidator<CreateRemediationTaskCommand>
{
    public CreateRemediationTaskCommandValidator()
    {
        RuleFor(x => x.FindingId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
    }
}

public sealed class CreateRemediationTaskCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, INotificationService notificationService, IAuditLogger auditLogger)
    : IRequestHandler<CreateRemediationTaskCommand, RemediationTaskDetailDto>
{
    public async Task<RemediationTaskDetailDto> Handle(CreateRemediationTaskCommand request, CancellationToken cancellationToken)
    {
        var finding = await db.Findings.FirstOrDefaultAsync(f => f.Id == request.FindingId, cancellationToken)
            ?? throw new NotFoundException(nameof(Finding), request.FindingId);

        User? owner = null;
        if (request.OwnerUserId is { } ownerId)
        {
            owner = await db.Users.FirstOrDefaultAsync(u => u.Id == ownerId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), ownerId);
        }

        var task = new RemediationTask
        {
            OrganisationId = finding.OrganisationId,
            FindingId = finding.Id,
            Finding = finding,
            Title = request.Title.Trim(),
            Description = request.Description,
            OwnerUserId = owner?.Id,
            Owner = owner,
            DueDate = request.DueDate,
            Status = RemediationStatus.OPEN,
        };

        db.RemediationTasks.Add(task);

        // A remediation task existing means work has started on this finding.
        if (finding.Status == FindingStatus.OPEN || finding.Status == FindingStatus.ASSIGNED)
        {
            finding.Status = FindingStatus.IN_PROGRESS;
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("remediation.task_created", nameof(RemediationTask), task.Id.ToString(), newValue: new { task.Title, task.FindingId }, cancellationToken: cancellationToken);

        if (owner is not null)
        {
            await notificationService.NotifyAsync(new NotificationMessage(
                owner.Id, "remediation.assigned", $"Remediation task assigned: {task.Title}",
                $"You have been assigned a remediation task for finding {FindingMapper.DisplayNumber(finding)}."), cancellationToken);
        }

        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);
        return RemediationMapper.ToDetailDto(task, today);
    }
}
