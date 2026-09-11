using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.UpdateControl;

/// <summary>Increments Control.Version on every update — the control's own change-tracking counter, distinct from ContentReviewStatus.</summary>
public sealed record UpdateControlCommand(
    Guid Id,
    string Name,
    string Description,
    string Objective,
    Guid ControlCategoryId,
    string RiskLevel,
    string? ApplicableConditions,
    string? EvidenceRequirementsSummary,
    string? Guidance,
    string SourceReference,
    DateOnly? EffectiveDate,
    DateOnly? ReviewDate,
    string ReviewStatus) : IRequest<ControlDetailDto>;

public sealed class UpdateControlCommandValidator : AbstractValidator<UpdateControlCommand>
{
    public UpdateControlCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Objective).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.SourceReference).NotEmpty().MaximumLength(500);
        RuleFor(x => x.RiskLevel)
            .Must(level => Enum.TryParse<Domain.Modules.Compliance.RiskLevel>(level, out _))
            .WithMessage("riskLevel must be one of: " + string.Join(", ", Enum.GetNames<Domain.Modules.Compliance.RiskLevel>()));
        RuleFor(x => x.ReviewStatus)
            .Must(status => Enum.TryParse<ContentReviewStatus>(status, out _))
            .WithMessage("reviewStatus must be one of: " + string.Join(", ", Enum.GetNames<ContentReviewStatus>()));
    }
}

public sealed class UpdateControlCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<UpdateControlCommand, ControlDetailDto>
{
    public async Task<ControlDetailDto> Handle(UpdateControlCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance control library.");
        }

        var control = await db.Controls
            .Include(c => c.ControlCategory)
            .Include(c => c.ControlMappings).ThenInclude(m => m.Requirement).ThenInclude(r => r.LegalReference)
            .Include(c => c.Questions).ThenInclude(q => q.EvidenceRequirements)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Control), request.Id);

        var category = await db.ControlCategories.FirstOrDefaultAsync(cat => cat.Id == request.ControlCategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(ControlCategory), request.ControlCategoryId);

        var oldValue = new { control.Name, control.Status, control.Version };

        control.Name = request.Name.Trim();
        control.Description = request.Description.Trim();
        control.Objective = request.Objective.Trim();
        control.ControlCategoryId = request.ControlCategoryId;
        control.RiskLevel = Enum.Parse<Domain.Modules.Compliance.RiskLevel>(request.RiskLevel);
        control.ApplicableConditions = request.ApplicableConditions;
        control.EvidenceRequirementsSummary = request.EvidenceRequirementsSummary;
        control.Guidance = request.Guidance;
        control.SourceReference = request.SourceReference.Trim();
        control.EffectiveDate = request.EffectiveDate;
        control.ReviewDate = request.ReviewDate;
        control.ReviewStatus = Enum.Parse<ContentReviewStatus>(request.ReviewStatus);
        control.Version += 1;
        control.ControlCategory = category;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "compliance.control_updated",
            nameof(Control),
            control.Id.ToString(),
            oldValue,
            new { control.Name, control.Status, control.Version },
            cancellationToken);

        return ComplianceMapper.ToDetailDto(control);
    }
}
