using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.CreateEvidenceRequirement;

public sealed record CreateEvidenceRequirementCommand(
    Guid AssessmentQuestionId,
    string Name,
    string? Description,
    bool IsMandatory,
    string? AcceptableFormats) : IRequest<EvidenceRequirementDto>;

public sealed class CreateEvidenceRequirementCommandValidator : AbstractValidator<CreateEvidenceRequirementCommand>
{
    public CreateEvidenceRequirementCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
    }
}

public sealed class CreateEvidenceRequirementCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateEvidenceRequirementCommand, EvidenceRequirementDto>
{
    public async Task<EvidenceRequirementDto> Handle(CreateEvidenceRequirementCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance control library.");
        }

        var question = await db.AssessmentQuestions.FirstOrDefaultAsync(q => q.Id == request.AssessmentQuestionId, cancellationToken)
            ?? throw new NotFoundException(nameof(AssessmentQuestion), request.AssessmentQuestionId);

        var evidence = new EvidenceRequirement
        {
            AssessmentQuestionId = request.AssessmentQuestionId,
            Name = request.Name.Trim(),
            Description = request.Description,
            IsMandatory = request.IsMandatory,
            AcceptableFormats = request.AcceptableFormats,
        };

        db.EvidenceRequirements.Add(evidence);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.evidence_requirement_created", nameof(EvidenceRequirement), evidence.Id.ToString(), newValue: new { evidence.Name }, cancellationToken: cancellationToken);

        evidence.AssessmentQuestion = question;
        return ComplianceMapper.ToDto(evidence);
    }
}
