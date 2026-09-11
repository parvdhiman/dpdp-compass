using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.Commands.UpdateAssessment;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Assessments.Commands.ReviewAssessment;

/// <summary>Records one review pass. The first call while SUBMITTED moves the assessment to UNDER_REVIEW; further calls while UNDER_REVIEW just add another round of feedback.</summary>
public sealed record ReviewAssessmentCommand(Guid Id, string Decision, string? Comments) : IRequest<AssessmentDetailDto>;

public sealed class ReviewAssessmentCommandValidator : AbstractValidator<ReviewAssessmentCommand>
{
    public ReviewAssessmentCommandValidator()
    {
        RuleFor(x => x.Decision)
            .Must(d => Enum.TryParse<ReviewDecision>(d, out _))
            .WithMessage("decision must be one of: " + string.Join(", ", Enum.GetNames<ReviewDecision>()));
        RuleFor(x => x.Comments).MaximumLength(2000);
    }
}

public sealed class ReviewAssessmentCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<ReviewAssessmentCommand, AssessmentDetailDto>
{
    public async Task<AssessmentDetailDto> Handle(ReviewAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await UpdateAssessmentCommandHandler.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (assessment.Status is not (AssessmentStatus.SUBMITTED or AssessmentStatus.UNDER_REVIEW))
        {
            throw new ConflictException($"Cannot review an assessment in {assessment.Status} status.");
        }

        if (assessment.Status == AssessmentStatus.SUBMITTED)
        {
            assessment.Status = AssessmentStatus.UNDER_REVIEW;
        }

        var reviewer = await db.Users.FirstAsync(u => u.Id == currentUser.UserId!.Value, cancellationToken);

        var review = new AssessmentReview
        {
            OrganisationId = assessment.OrganisationId,
            AssessmentId = assessment.Id,
            ReviewerId = reviewer.Id,
            Reviewer = reviewer,
            Decision = Enum.Parse<ReviewDecision>(request.Decision),
            Comments = request.Comments,
            CreatedAt = dateTimeProvider.UtcNow,
        };
        assessment.Reviews.Add(review);
        // Explicit Add is required even though it's already reachable via
        // assessment.Reviews: Entity.Id is a client-generated, non-default
        // Guid by the time SaveChanges runs its own change-detection graph
        // walk, so a new entity discovered *only* through navigation fixup
        // gets misdetected as Modified (an existing, unchanged row) instead
        // of Added — see docs/ARCHITECTURE.md Module 5 section.
        db.AssessmentReviews.Add(review);

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "assessments.reviewed",
            nameof(Assessment),
            assessment.Id.ToString(),
            newValue: new { review.Decision },
            cancellationToken: cancellationToken);

        return AssessmentMapper.ToDetailDto(assessment);
    }
}
