using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.UpdateEvidenceRequirement;

public sealed record UpdateEvidenceRequirementCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsMandatory,
    string? AcceptableFormats) : IRequest<EvidenceRequirementDto>;

public sealed class UpdateEvidenceRequirementCommandValidator : AbstractValidator<UpdateEvidenceRequirementCommand>
{
    public UpdateEvidenceRequirementCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
    }
}

public sealed class UpdateEvidenceRequirementCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<UpdateEvidenceRequirementCommand, EvidenceRequirementDto>
{
    public async Task<EvidenceRequirementDto> Handle(UpdateEvidenceRequirementCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance control library.");
        }

        var evidence = await db.EvidenceRequirements
            .Include(e => e.AssessmentQuestion)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(EvidenceRequirement), request.Id);

        evidence.Name = request.Name.Trim();
        evidence.Description = request.Description;
        evidence.IsMandatory = request.IsMandatory;
        evidence.AcceptableFormats = request.AcceptableFormats;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.evidence_requirement_updated", nameof(EvidenceRequirement), evidence.Id.ToString(), cancellationToken: cancellationToken);

        return ComplianceMapper.ToDto(evidence);
    }
}
