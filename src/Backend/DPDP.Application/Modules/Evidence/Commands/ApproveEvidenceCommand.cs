using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Evidence.DTOs;
using DPDP.Domain.Modules.Evidence;
using MediatR;

namespace DPDP.Application.Modules.Evidence.Commands;

public sealed record ApproveEvidenceCommand(Guid Id, string? Comments) : IRequest<EvidenceDetailDto>;

public sealed class ApproveEvidenceCommandHandler(
    IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider,
    INotificationService notificationService, IAuditLogger auditLogger)
    : IRequestHandler<ApproveEvidenceCommand, EvidenceDetailDto>
{
    public async Task<EvidenceDetailDto> Handle(ApproveEvidenceCommand request, CancellationToken cancellationToken)
    {
        var evidence = await EvidenceLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!EvidenceStatusTransitions.CanTransition(evidence.Status, EvidenceStatus.APPROVED))
        {
            throw new ConflictException($"Cannot approve evidence in {evidence.Status} status.");
        }

        var now = dateTimeProvider.UtcNow;
        var currentVersionNumber = evidence.Versions.Max(v => v.VersionNumber);

        evidence.Status = EvidenceStatus.APPROVED;
        evidence.ApprovedAt = now;
        evidence.ApprovedBy = currentUser.UserId;
        evidence.RejectedAt = null;
        evidence.RejectedBy = null;
        evidence.RejectionReason = null;

        var review = new EvidenceReviewRecord
        {
            OrganisationId = evidence.OrganisationId,
            EvidenceItem = evidence,
            EvidenceVersionNumber = currentVersionNumber,
            ReviewerUserId = currentUser.UserId!.Value,
            Decision = EvidenceReviewDecision.APPROVED,
            Comments = request.Comments,
            CreatedAt = now,
        };
        evidence.Reviews.Add(review);
        db.EvidenceReviewRecords.Add(review);

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("evidence.approved", nameof(EvidenceItem), evidence.Id.ToString(), cancellationToken: cancellationToken);

        if (evidence.OwnerUserId is { } ownerId)
        {
            await notificationService.NotifyAsync(new NotificationMessage(
                ownerId, "evidence.approved", $"Evidence approved: {evidence.Title}",
                $"Evidence {EvidenceMapper.DisplayNumber(evidence)} has been approved."), cancellationToken);
        }

        return EvidenceMapper.ToDetailDto(evidence, DateOnly.FromDateTime(now.UtcDateTime));
    }
}
