using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.Commands.UpdateAssessment;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Assessments.Commands.RejectAssessment;

/// <summary>Comments are required here (unlike approve) — a rejection must explain what needs to change before the assessor can act on it.</summary>
public sealed record RejectAssessmentCommand(Guid Id, string Comments) : IRequest<AssessmentDetailDto>;

public sealed class RejectAssessmentCommandValidator : AbstractValidator<RejectAssessmentCommand>
{
    public RejectAssessmentCommandValidator()
    {
        RuleFor(x => x.Comments).NotEmpty().MaximumLength(2000);
    }
}

public sealed class RejectAssessmentCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<RejectAssessmentCommand, AssessmentDetailDto>
{
    public async Task<AssessmentDetailDto> Handle(RejectAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await UpdateAssessmentCommandHandler.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (assessment.Status != AssessmentStatus.UNDER_REVIEW)
        {
            throw new ConflictException($"Cannot reject an assessment in {assessment.Status} status — it must be under review.");
        }

        var decider = await db.Users.FirstAsync(u => u.Id == currentUser.UserId!.Value, cancellationToken);
        var now = dateTimeProvider.UtcNow;

        assessment.Status = AssessmentStatus.REJECTED;
        assessment.DecidedAt = now;
        assessment.DecidedBy = decider.Id;

        var approval = new AssessmentApproval
        {
            OrganisationId = assessment.OrganisationId,
            AssessmentId = assessment.Id,
            DecidedByUserId = decider.Id,
            DecidedByUser = decider,
            Decision = ApprovalDecision.REJECTED,
            Comments = request.Comments,
            CreatedAt = now,
        };
        assessment.Approvals.Add(approval);
        // Explicit Add required — see ReviewAssessmentCommand's comment on the same pattern.
        db.AssessmentApprovals.Add(approval);

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("assessments.rejected", nameof(Assessment), assessment.Id.ToString(), cancellationToken: cancellationToken);

        return AssessmentMapper.ToDetailDto(assessment);
    }
}
