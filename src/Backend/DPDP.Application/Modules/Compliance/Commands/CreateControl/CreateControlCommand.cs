using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.CreateControl;

/// <summary>New controls always start DRAFT/DRAFT (ControlStatus and ContentReviewStatus) — see docs/COMPLIANCE_CONTENT_GOVERNANCE.md. Use ActivateControlCommand once ready.</summary>
public sealed record CreateControlCommand(
    string ControlId,
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
    DateOnly? ReviewDate) : IRequest<ControlDetailDto>;

public sealed class CreateControlCommandValidator : AbstractValidator<CreateControlCommand>
{
    public CreateControlCommandValidator()
    {
        RuleFor(x => x.ControlId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Objective).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.SourceReference).NotEmpty().MaximumLength(500);
        RuleFor(x => x.RiskLevel)
            .Must(level => Enum.TryParse<Domain.Modules.Compliance.RiskLevel>(level, out _))
            .WithMessage("riskLevel must be one of: " + string.Join(", ", Enum.GetNames<Domain.Modules.Compliance.RiskLevel>()));
    }
}

public sealed class CreateControlCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateControlCommand, ControlDetailDto>
{
    public async Task<ControlDetailDto> Handle(CreateControlCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance control library.");
        }

        var category = await db.ControlCategories.FirstOrDefaultAsync(c => c.Id == request.ControlCategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(ControlCategory), request.ControlCategoryId);

        var controlIdTaken = await db.Controls.AnyAsync(c => c.ControlId == request.ControlId, cancellationToken);
        if (controlIdTaken)
        {
            throw new ConflictException("A control with this Control ID already exists.");
        }

        var control = new Control
        {
            ControlId = request.ControlId.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Objective = request.Objective.Trim(),
            ControlCategoryId = request.ControlCategoryId,
            RiskLevel = Enum.Parse<Domain.Modules.Compliance.RiskLevel>(request.RiskLevel),
            ApplicableConditions = request.ApplicableConditions,
            EvidenceRequirementsSummary = request.EvidenceRequirementsSummary,
            Guidance = request.Guidance,
            SourceReference = request.SourceReference.Trim(),
            EffectiveDate = request.EffectiveDate,
            ReviewDate = request.ReviewDate,
            Version = 1,
            Status = ControlStatus.DRAFT,
            ReviewStatus = ContentReviewStatus.DRAFT,
        };

        db.Controls.Add(control);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.control_created", nameof(Control), control.Id.ToString(), newValue: new { control.ControlId, control.Name }, cancellationToken: cancellationToken);

        control.ControlCategory = category;
        return ComplianceMapper.ToDetailDto(control);
    }
}
