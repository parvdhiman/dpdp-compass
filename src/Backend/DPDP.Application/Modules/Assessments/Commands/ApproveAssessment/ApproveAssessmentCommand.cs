using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.Commands.UpdateAssessment;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Assessments.Commands.ApproveAssessment;

public sealed record ApproveAssessmentCommand(Guid Id, string? Comments) : IRequest<AssessmentDetailDto>;

public sealed class ApproveAssessmentCommandValidator : AbstractValidator<ApproveAssessmentCommand>
{
    public ApproveAssessmentCommandValidator()
    {
        RuleFor(x => x.Comments).MaximumLength(2000);
    }
}

public sealed class ApproveAssessmentCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<ApproveAssessmentCommand, AssessmentDetailDto>
{
    public async Task<AssessmentDetailDto> Handle(ApproveAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await UpdateAssessmentCommandHandler.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (assessment.Status != AssessmentStatus.UNDER_REVIEW)
        {
            throw new ConflictException($"Cannot approve an assessment in {assessment.Status} status — it must be under review.");
        }

        var decider = await db.Users.FirstAsync(u => u.Id == currentUser.UserId!.Value, cancellationToken);
        var now = dateTimeProvider.UtcNow;

        assessment.Status = AssessmentStatus.APPROVED;
        assessment.DecidedAt = now;
        assessment.DecidedBy = decider.Id;

        var approval = new AssessmentApproval
        {
            OrganisationId = assessment.OrganisationId,
            AssessmentId = assessment.Id,
            DecidedByUserId = decider.Id,
            DecidedByUser = decider,
            Decision = ApprovalDecision.APPROVED,
            Comments = request.Comments,
            CreatedAt = now,
        };
        assessment.Approvals.Add(approval);
        // Explicit Add required — see ReviewAssessmentCommand's comment on the same pattern.
        db.AssessmentApprovals.Add(approval);

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("assessments.approved", nameof(Assessment), assessment.Id.ToString(), cancellationToken: cancellationToken);

        return AssessmentMapper.ToDetailDto(assessment);
    }
}
