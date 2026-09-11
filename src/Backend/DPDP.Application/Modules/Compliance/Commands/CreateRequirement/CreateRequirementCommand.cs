using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.CreateRequirement;

public sealed record CreateRequirementCommand(
    Guid LegalReferenceId,
    string Code,
    string Title,
    string Description) : IRequest<RequirementDto>;

public sealed class CreateRequirementCommandValidator : AbstractValidator<CreateRequirementCommand>
{
    public CreateRequirementCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
    }
}

public sealed class CreateRequirementCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateRequirementCommand, RequirementDto>
{
    public async Task<RequirementDto> Handle(CreateRequirementCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance framework library.");
        }

        var legalReference = await db.LegalReferences.FirstOrDefaultAsync(l => l.Id == request.LegalReferenceId, cancellationToken)
            ?? throw new NotFoundException(nameof(LegalReference), request.LegalReferenceId);

        var codeTaken = await db.Requirements.AnyAsync(r => r.Code == request.Code, cancellationToken);
        if (codeTaken)
        {
            throw new ConflictException("A requirement with this code already exists.");
        }

        var requirement = new Requirement
        {
            LegalReferenceId = request.LegalReferenceId,
            Code = request.Code.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            ReviewStatus = ContentReviewStatus.DRAFT,
        };

        db.Requirements.Add(requirement);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.requirement_created", nameof(Requirement), requirement.Id.ToString(), newValue: new { requirement.Code }, cancellationToken: cancellationToken);

        return new RequirementDto(requirement.Id, requirement.LegalReferenceId, legalReference.Citation, legalReference.SourceCitation, requirement.Code, requirement.Title, requirement.Description, requirement.ReviewStatus.ToString(), []);
    }
}
