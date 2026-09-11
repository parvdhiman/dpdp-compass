using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.Commands.UpdateAssessment;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Assessments.Commands.AssignAssessment;

public sealed record AssignAssessmentCommand(Guid Id, Guid? AssignedToUserId) : IRequest<AssessmentDetailDto>;

public sealed class AssignAssessmentCommandValidator : AbstractValidator<AssignAssessmentCommand>
{
    public AssignAssessmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class AssignAssessmentCommandHandler(IAppDbContext db, IAuditLogger auditLogger)
    : IRequestHandler<AssignAssessmentCommand, AssessmentDetailDto>
{
    public async Task<AssessmentDetailDto> Handle(AssignAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await UpdateAssessmentCommandHandler.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (assessment.Status is AssessmentStatus.APPROVED or AssessmentStatus.ARCHIVED)
        {
            throw new ConflictException("Cannot reassign an assessment that has already been approved or archived.");
        }

        if (request.AssignedToUserId is { } userId)
        {
            var assignee = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), userId);
            assessment.AssignedToUserId = assignee.Id;
            assessment.AssignedToUser = assignee;
        }
        else
        {
            assessment.AssignedToUserId = null;
            assessment.AssignedToUser = null;
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "assessments.assigned",
            nameof(Assessment),
            assessment.Id.ToString(),
            newValue: new { assessment.AssignedToUserId },
            cancellationToken: cancellationToken);

        return AssessmentMapper.ToDetailDto(assessment);
    }
}
