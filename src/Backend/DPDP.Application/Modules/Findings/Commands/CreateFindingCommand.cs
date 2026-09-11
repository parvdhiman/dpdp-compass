using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Findings.DTOs;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Findings;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Findings.Commands;

public sealed record CreateFindingCommand(
    string Title,
    string Description,
    string Severity,
    Guid? ControlId,
    string? AssetReference,
    Guid? OwnerUserId,
    DateOnly? DueDate,
    string? Recommendation) : IRequest<FindingDetailDto>;

public sealed class CreateFindingCommandValidator : AbstractValidator<CreateFindingCommand>
{
    public CreateFindingCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Severity).Must(v => Enum.TryParse<FindingSeverity>(v, out _))
            .WithMessage("severity must be one of: " + string.Join(", ", Enum.GetNames<FindingSeverity>()));
        RuleFor(x => x.Recommendation).MaximumLength(2000);
    }
}

/// <summary>Manual finding creation — not derived from an assessment. See CreateFindingFromAssessmentControlCommand for the "Assessment -&gt; Failed/Partial Control -&gt; Finding" auto-creation path.</summary>
public sealed class CreateFindingCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, INotificationService notificationService, IAuditLogger auditLogger)
    : IRequestHandler<CreateFindingCommand, FindingDetailDto>
{
    public async Task<FindingDetailDto> Handle(CreateFindingCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a finding.");
        }

        Control? control = null;
        if (request.ControlId is { } controlId)
        {
            control = await db.Controls.FirstOrDefaultAsync(c => c.Id == controlId, cancellationToken)
                ?? throw new NotFoundException(nameof(Control), controlId);
        }

        User? owner = null;
        if (request.OwnerUserId is { } ownerId)
        {
            owner = await db.Users.FirstOrDefaultAsync(u => u.Id == ownerId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), ownerId);
        }

        var finding = new Finding
        {
            OrganisationId = organisationId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Source = FindingSource.MANUAL,
            ControlId = control?.Id,
            Control = control,
            AssetReference = request.AssetReference,
            Severity = Enum.Parse<FindingSeverity>(request.Severity),
            OwnerUserId = owner?.Id,
            Owner = owner,
            DueDate = request.DueDate,
            Status = owner is null ? FindingStatus.OPEN : FindingStatus.ASSIGNED,
            Recommendation = request.Recommendation,
        };

        db.Findings.Add(finding);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("findings.created", nameof(Finding), finding.Id.ToString(), newValue: new { finding.Title, finding.Severity }, cancellationToken: cancellationToken);

        if (owner is not null)
        {
            await notificationService.NotifyAsync(new NotificationMessage(
                owner.Id, "finding.assigned", $"Finding assigned: {finding.Title}",
                $"You have been assigned finding {FindingMapper.DisplayNumber(finding)} ({finding.Severity})."), cancellationToken);
        }

        return FindingMapper.ToDetailDto(finding);
    }
}
