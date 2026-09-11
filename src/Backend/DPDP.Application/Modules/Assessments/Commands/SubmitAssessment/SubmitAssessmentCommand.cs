using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.Commands.UpdateAssessment;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using DPDP.Domain.Modules.Compliance;
using MediatR;

namespace DPDP.Application.Modules.Assessments.Commands.SubmitAssessment;

public sealed record SubmitAssessmentCommand(Guid Id) : IRequest<AssessmentDetailDto>;

public sealed class SubmitAssessmentCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<SubmitAssessmentCommand, AssessmentDetailDto>
{
    public async Task<AssessmentDetailDto> Handle(SubmitAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await UpdateAssessmentCommandHandler.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (assessment.Status is not (AssessmentStatus.DRAFT or AssessmentStatus.IN_PROGRESS))
        {
            throw new ConflictException($"Cannot submit an assessment in {assessment.Status} status.");
        }

        var unansweredRequiredCount = assessment.Controls
            .Where(c => c.Status != AnswerStatus.NOT_APPLICABLE)
            .SelectMany(c => c.Questions)
            .Count(q => q.Question.IsRequired && q.Answer!.Status == AnswerStatus.NOT_ASSESSED);

        if (unansweredRequiredCount > 0)
        {
            throw new ConflictException($"Cannot submit: {unansweredRequiredCount} required question(s) are still unanswered.");
        }

        assessment.Status = AssessmentStatus.SUBMITTED;
        assessment.SubmittedAt = dateTimeProvider.UtcNow;
        assessment.SubmittedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("assessments.submitted", nameof(Assessment), assessment.Id.ToString(), cancellationToken: cancellationToken);

        return AssessmentMapper.ToDetailDto(assessment);
    }
}
