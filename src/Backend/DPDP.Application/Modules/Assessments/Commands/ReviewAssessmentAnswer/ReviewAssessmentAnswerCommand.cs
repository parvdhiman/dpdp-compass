using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.Commands.SaveAssessmentAnswer;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Assessments.Commands.ReviewAssessmentAnswer;

/// <summary>Lets a reviewer annotate one specific answer (the Reviewer Screen's per-question row) — distinct from ReviewAssessmentCommand, which records the overall review pass.</summary>
public sealed record ReviewAssessmentAnswerCommand(Guid AssessmentControlQuestionId, string? ReviewComment, bool FlagForReview) : IRequest<AssessmentAnswerDto>;

public sealed class ReviewAssessmentAnswerCommandValidator : AbstractValidator<ReviewAssessmentAnswerCommand>
{
    public ReviewAssessmentAnswerCommandValidator()
    {
        RuleFor(x => x.AssessmentControlQuestionId).NotEmpty();
        RuleFor(x => x.ReviewComment).MaximumLength(2000);
    }
}

public sealed class ReviewAssessmentAnswerCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<ReviewAssessmentAnswerCommand, AssessmentAnswerDto>
{
    public async Task<AssessmentAnswerDto> Handle(ReviewAssessmentAnswerCommand request, CancellationToken cancellationToken)
    {
        var controlQuestion = await db.AssessmentControlQuestions
            .Include(q => q.Question).ThenInclude(cq => cq.EvidenceRequirements)
            .Include(q => q.Answer)
            .Include(q => q.AssessmentControl).ThenInclude(c => c.Assessment)
            .Include(q => q.AssessmentControl).ThenInclude(c => c.Questions).ThenInclude(sibling => sibling.Question)
            .Include(q => q.AssessmentControl).ThenInclude(c => c.Questions).ThenInclude(sibling => sibling.Answer)
            .FirstOrDefaultAsync(q => q.Id == request.AssessmentControlQuestionId, cancellationToken)
            ?? throw new NotFoundException(nameof(AssessmentControlQuestion), request.AssessmentControlQuestionId);

        var assessment = controlQuestion.AssessmentControl.Assessment;
        if (assessment.Status is not (AssessmentStatus.SUBMITTED or AssessmentStatus.UNDER_REVIEW))
        {
            throw new ConflictException("Answers can only be reviewed once an assessment has been submitted.");
        }

        var answer = controlQuestion.Answer!;
        answer.ReviewerId = currentUser.UserId;
        answer.ReviewedAt = dateTimeProvider.UtcNow;
        answer.ReviewComment = request.ReviewComment;

        if (request.FlagForReview)
        {
            answer.Status = AnswerStatus.NEEDS_REVIEW;
            SaveAssessmentAnswerCommandHandler.RecalculateControlStatus(controlQuestion.AssessmentControl);
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "assessments.answer_reviewed",
            nameof(AssessmentAnswer),
            answer.Id.ToString(),
            newValue: new { request.FlagForReview },
            cancellationToken: cancellationToken);

        return AssessmentMapper.ToAnswerDto(controlQuestion);
    }
}
