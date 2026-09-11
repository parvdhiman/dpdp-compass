using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Evidence.DTOs;
using DPDP.Domain.Modules.Evidence;
using MediatR;

namespace DPDP.Application.Modules.Evidence.Commands;

public sealed record SubmitEvidenceForReviewCommand(Guid Id) : IRequest<EvidenceDetailDto>;

public sealed class SubmitEvidenceForReviewCommandHandler(
    IAppDbContext db, IDateTimeProvider dateTimeProvider, INotificationService notificationService, IAuditLogger auditLogger)
    : IRequestHandler<SubmitEvidenceForReviewCommand, EvidenceDetailDto>
{
    public async Task<EvidenceDetailDto> Handle(SubmitEvidenceForReviewCommand request, CancellationToken cancellationToken)
    {
        var evidence = await EvidenceLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!EvidenceStatusTransitions.CanTransition(evidence.Status, EvidenceStatus.UNDER_REVIEW))
        {
            throw new ConflictException($"Cannot submit evidence in {evidence.Status} status for review.");
        }

        if (evidence.ReviewerUserId is null)
        {
            throw new ConflictException("A reviewer must be assigned before submitting evidence for review.");
        }

        evidence.Status = EvidenceStatus.UNDER_REVIEW;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("evidence.submitted_for_review", nameof(EvidenceItem), evidence.Id.ToString(), cancellationToken: cancellationToken);

        await notificationService.NotifyAsync(new NotificationMessage(
            evidence.ReviewerUserId.Value, "evidence.review_requested", $"Evidence ready for review: {evidence.Title}",
            $"Evidence {EvidenceMapper.DisplayNumber(evidence)} has been submitted for your review."), cancellationToken);

        return EvidenceMapper.ToDetailDto(evidence, DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime));
    }
}
