using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.Commands.UpdateAssessment;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using MediatR;

namespace DPDP.Application.Modules.Assessments.Commands.ReopenAssessment;

/// <summary>Only a REJECTED assessment can be reopened — back to IN_PROGRESS so the assessor can fix and resubmit. Clears the submission/decision markers so they reflect the fresh cycle.</summary>
public sealed record ReopenAssessmentCommand(Guid Id) : IRequest<AssessmentDetailDto>;

public sealed class ReopenAssessmentCommandHandler(IAppDbContext db, IAuditLogger auditLogger)
    : IRequestHandler<ReopenAssessmentCommand, AssessmentDetailDto>
{
    public async Task<AssessmentDetailDto> Handle(ReopenAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await UpdateAssessmentCommandHandler.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (assessment.Status != AssessmentStatus.REJECTED)
        {
            throw new ConflictException("Only a rejected assessment can be reopened.");
        }

        assessment.Status = AssessmentStatus.IN_PROGRESS;
        assessment.SubmittedAt = null;
        assessment.SubmittedBy = null;
        assessment.DecidedAt = null;
        assessment.DecidedBy = null;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("assessments.reopened", nameof(Assessment), assessment.Id.ToString(), cancellationToken: cancellationToken);

        return AssessmentMapper.ToDetailDto(assessment);
    }
}
