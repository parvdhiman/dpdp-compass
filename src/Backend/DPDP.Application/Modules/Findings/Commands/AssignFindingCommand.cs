using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Findings.DTOs;
using DPDP.Domain.Modules.Findings;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Findings.Commands;

public sealed record AssignFindingCommand(Guid Id, Guid OwnerUserId) : IRequest<FindingDetailDto>;

public sealed class AssignFindingCommandValidator : AbstractValidator<AssignFindingCommand>
{
    public AssignFindingCommandValidator()
    {
        RuleFor(x => x.OwnerUserId).NotEmpty();
    }
}

public sealed class AssignFindingCommandHandler(IAppDbContext db, INotificationService notificationService, IAuditLogger auditLogger)
    : IRequestHandler<AssignFindingCommand, FindingDetailDto>
{
    public async Task<FindingDetailDto> Handle(AssignFindingCommand request, CancellationToken cancellationToken)
    {
        var finding = await FindingLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (finding.Status is FindingStatus.CLOSED or FindingStatus.ACCEPTED_RISK)
        {
            throw new ConflictException($"Cannot assign a finding in {finding.Status} status.");
        }

        var owner = await db.Users.FirstOrDefaultAsync(u => u.Id == request.OwnerUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.OwnerUserId);

        finding.OwnerUserId = owner.Id;
        finding.Owner = owner;
        if (finding.Status == FindingStatus.OPEN)
        {
            finding.Status = FindingStatus.ASSIGNED;
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("findings.assigned", nameof(Finding), finding.Id.ToString(), newValue: new { finding.OwnerUserId }, cancellationToken: cancellationToken);

        await notificationService.NotifyAsync(new NotificationMessage(
            owner.Id, "finding.assigned", $"Finding assigned: {finding.Title}",
            $"You have been assigned finding {FindingMapper.DisplayNumber(finding)} ({finding.Severity})."), cancellationToken);

        return FindingMapper.ToDetailDto(finding);
    }
}
