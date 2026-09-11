using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.Commands.UpdateAssessment;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using MediatR;

namespace DPDP.Application.Modules.Assessments.Commands.ArchiveAssessment;

public sealed record ArchiveAssessmentCommand(Guid Id) : IRequest<AssessmentDetailDto>;

public sealed class ArchiveAssessmentCommandHandler(IAppDbContext db, IAuditLogger auditLogger)
    : IRequestHandler<ArchiveAssessmentCommand, AssessmentDetailDto>
{
    public async Task<AssessmentDetailDto> Handle(ArchiveAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await UpdateAssessmentCommandHandler.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (assessment.Status != AssessmentStatus.APPROVED)
        {
            throw new ConflictException("Only an approved assessment can be archived.");
        }

        assessment.Status = AssessmentStatus.ARCHIVED;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("assessments.archived", nameof(Assessment), assessment.Id.ToString(), cancellationToken: cancellationToken);

        return AssessmentMapper.ToDetailDto(assessment);
    }
}
