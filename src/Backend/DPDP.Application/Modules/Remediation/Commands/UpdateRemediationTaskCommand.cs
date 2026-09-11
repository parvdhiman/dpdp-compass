using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Remediation.DTOs;
using FluentValidation;
using MediatR;

namespace DPDP.Application.Modules.Remediation.Commands;

public sealed record UpdateRemediationTaskCommand(Guid Id, string Title, string? Description, DateOnly? DueDate) : IRequest<RemediationTaskDetailDto>;

public sealed class UpdateRemediationTaskCommandValidator : AbstractValidator<UpdateRemediationTaskCommand>
{
    public UpdateRemediationTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
    }
}

public sealed class UpdateRemediationTaskCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<UpdateRemediationTaskCommand, RemediationTaskDetailDto>
{
    public async Task<RemediationTaskDetailDto> Handle(UpdateRemediationTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await RemediationTaskLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        task.Title = request.Title.Trim();
        task.Description = request.Description;
        task.DueDate = request.DueDate;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("remediation.task_updated", nameof(Domain.Modules.Remediation.RemediationTask), task.Id.ToString(), cancellationToken: cancellationToken);

        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);
        return RemediationMapper.ToDetailDto(task, today);
    }
}
