using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Evidence.DTOs;
using DPDP.Domain.Modules.Evidence;
using FluentValidation;
using MediatR;

namespace DPDP.Application.Modules.Evidence.Commands;

public sealed record RejectEvidenceCommand(Guid Id, string RejectionReason) : IRequest<EvidenceDetailDto>;

public sealed class RejectEvidenceCommandValidator : AbstractValidator<RejectEvidenceCommand>
{
    public RejectEvidenceCommandValidator()
    {
        RuleFor(x => x.RejectionReason).NotEmpty().MaximumLength(2000);
    }
}

public sealed class RejectEvidenceCommandHandler(
    IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider,
    INotificationService notificationService, IAuditLogger auditLogger)
    : IRequestHandler<RejectEvidenceCommand, EvidenceDetailDto>
{
    public async Task<EvidenceDetailDto> Handle(RejectEvidenceCommand request, CancellationToken cancellationToken)
    {
        var evidence = await EvidenceLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!EvidenceStatusTransitions.CanTransition(evidence.Status, EvidenceStatus.REJECTED))
        {
            throw new ConflictException($"Cannot reject evidence in {evidence.Status} status.");
        }

        var now = dateTimeProvider.UtcNow;
        var currentVersionNumber = evidence.Versions.Max(v => v.VersionNumber);

        evidence.Status = EvidenceStatus.REJECTED;
        evidence.RejectedAt = now;
        evidence.RejectedBy = currentUser.UserId;
        evidence.RejectionReason = request.RejectionReason;

        var review = new EvidenceReviewRecord
        {
            OrganisationId = evidence.OrganisationId,
            EvidenceItem = evidence,
            EvidenceVersionNumber = currentVersionNumber,
            ReviewerUserId = currentUser.UserId!.Value,
            Decision = EvidenceReviewDecision.REJECTED,
            Comments = request.RejectionReason,
            CreatedAt = now,
        };
        evidence.Reviews.Add(review);
        db.EvidenceReviewRecords.Add(review);

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "evidence.rejected", nameof(EvidenceItem), evidence.Id.ToString(),
            newValue: new { evidence.RejectionReason }, cancellationToken: cancellationToken);

        if (evidence.OwnerUserId is { } ownerId)
        {
            await notificationService.NotifyAsync(new NotificationMessage(
                ownerId, "evidence.rejected", $"Evidence rejected: {evidence.Title}",
                $"Evidence {EvidenceMapper.DisplayNumber(evidence)} was rejected: {request.RejectionReason}"), cancellationToken);
        }

        return EvidenceMapper.ToDetailDto(evidence, DateOnly.FromDateTime(now.UtcDateTime));
    }
}
