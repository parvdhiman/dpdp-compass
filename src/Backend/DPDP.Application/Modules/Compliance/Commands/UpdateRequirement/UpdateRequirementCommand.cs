using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.UpdateRequirement;

public sealed record UpdateRequirementCommand(Guid Id, string Title, string Description, string ReviewStatus) : IRequest<RequirementDto>;

public sealed class UpdateRequirementCommandValidator : AbstractValidator<UpdateRequirementCommand>
{
    public UpdateRequirementCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ReviewStatus)
            .Must(status => Enum.TryParse<ContentReviewStatus>(status, out _))
            .WithMessage("reviewStatus must be one of: " + string.Join(", ", Enum.GetNames<ContentReviewStatus>()));
    }
}

public sealed class UpdateRequirementCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<UpdateRequirementCommand, RequirementDto>
{
    public async Task<RequirementDto> Handle(UpdateRequirementCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance framework library.");
        }

        var requirement = await db.Requirements
            .Include(r => r.LegalReference)
            .Include(r => r.ControlMappings).ThenInclude(m => m.Control).ThenInclude(c => c.ControlCategory)
            .Include(r => r.ControlMappings).ThenInclude(m => m.Control).ThenInclude(c => c.Questions)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Requirement), request.Id);

        requirement.Title = request.Title.Trim();
        requirement.Description = request.Description.Trim();
        requirement.ReviewStatus = Enum.Parse<ContentReviewStatus>(request.ReviewStatus);

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.requirement_updated", nameof(Requirement), requirement.Id.ToString(), cancellationToken: cancellationToken);

        return new RequirementDto(
            requirement.Id, requirement.LegalReferenceId, requirement.LegalReference.Citation, requirement.LegalReference.SourceCitation,
            requirement.Code, requirement.Title, requirement.Description, requirement.ReviewStatus.ToString(),
            requirement.ControlMappings.Select(m => ComplianceMapper.ToSummaryDto(m.Control)).ToList());
    }
}
