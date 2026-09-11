using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Remediation.DTOs;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Remediation.Commands;

public sealed record AssignRemediationTaskCommand(Guid Id, Guid OwnerUserId) : IRequest<RemediationTaskDetailDto>;

public sealed class AssignRemediationTaskCommandValidator : AbstractValidator<AssignRemediationTaskCommand>
{
    public AssignRemediationTaskCommandValidator()
    {
        RuleFor(x => x.OwnerUserId).NotEmpty();
    }
}

public sealed class AssignRemediationTaskCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, INotificationService notificationService, IAuditLogger auditLogger)
    : IRequestHandler<AssignRemediationTaskCommand, RemediationTaskDetailDto>
{
    public async Task<RemediationTaskDetailDto> Handle(AssignRemediationTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await RemediationTaskLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        var owner = await db.Users.FirstOrDefaultAsync(u => u.Id == request.OwnerUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.OwnerUserId);

        task.OwnerUserId = owner.Id;
        task.Owner = owner;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("remediation.task_assigned", nameof(Domain.Modules.Remediation.RemediationTask), task.Id.ToString(), newValue: new { task.OwnerUserId }, cancellationToken: cancellationToken);

        await notificationService.NotifyAsync(new NotificationMessage(
            owner.Id, "remediation.assigned", $"Remediation task assigned: {task.Title}",
            "You have been assigned a remediation task."), cancellationToken);

        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);
        return RemediationMapper.ToDetailDto(task, today);
    }
}
