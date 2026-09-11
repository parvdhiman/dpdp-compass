using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Domain.Modules.DataInventory;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.DataInventory.Commands;

internal static class ProcessingActivityLoader
{
    public static async Task<ProcessingActivity> LoadForDetailAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.ProcessingActivities
            .Include(a => a.RetentionPolicy)
            .Include(a => a.Owner)
            .Include(a => a.DataCategories)
            .Include(a => a.ItSystems)
            .Include(a => a.DataCollectionSources)
            .Include(a => a.Recipients)
            .Include(a => a.Processors)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(ProcessingActivity), id);

    /// <summary>Replaces every many-to-many set wholesale — simpler and safe for a form-driven "save the whole set" UI, rather than diffing add/remove.</summary>
    public static async Task SetRelationshipsAsync(
        IAppDbContext db, ProcessingActivity activity,
        IReadOnlyList<Guid> dataCategoryIds, IReadOnlyList<Guid> itSystemIds, IReadOnlyList<Guid> dataCollectionSourceIds,
        IReadOnlyList<Guid> recipientIds, IReadOnlyList<Guid> processorIds, CancellationToken cancellationToken)
    {
        activity.DataCategories.Clear();
        foreach (var id in dataCategoryIds.Distinct())
        {
            activity.DataCategories.Add(await db.DataCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken) ?? throw new NotFoundException(nameof(DataCategory), id));
        }

        activity.ItSystems.Clear();
        foreach (var id in itSystemIds.Distinct())
        {
            activity.ItSystems.Add(await db.ItSystems.FirstOrDefaultAsync(s => s.Id == id, cancellationToken) ?? throw new NotFoundException(nameof(ItSystem), id));
        }

        activity.DataCollectionSources.Clear();
        foreach (var id in dataCollectionSourceIds.Distinct())
        {
            activity.DataCollectionSources.Add(await db.DataCollectionSources.FirstOrDefaultAsync(s => s.Id == id, cancellationToken) ?? throw new NotFoundException(nameof(DataCollectionSource), id));
        }

        activity.Recipients.Clear();
        foreach (var id in recipientIds.Distinct())
        {
            activity.Recipients.Add(await db.Recipients.FirstOrDefaultAsync(r => r.Id == id, cancellationToken) ?? throw new NotFoundException(nameof(Recipient), id));
        }

        activity.Processors.Clear();
        foreach (var id in processorIds.Distinct())
        {
            activity.Processors.Add(await db.Processors.FirstOrDefaultAsync(p => p.Id == id, cancellationToken) ?? throw new NotFoundException(nameof(Processor), id));
        }
    }
}

public sealed record CreateProcessingActivityCommand(
    string Name, string Purpose, IReadOnlyList<string> DataSubjectCategories, IReadOnlyList<string> SecurityControls,
    Guid? RetentionPolicyId, Guid? OwnerUserId, DateOnly? ReviewDate,
    IReadOnlyList<Guid> DataCategoryIds, IReadOnlyList<Guid> ItSystemIds, IReadOnlyList<Guid> DataCollectionSourceIds,
    IReadOnlyList<Guid> RecipientIds, IReadOnlyList<Guid> ProcessorIds) : IRequest<ProcessingActivityDetailDto>;

public sealed class CreateProcessingActivityCommandValidator : AbstractValidator<CreateProcessingActivityCommand>
{
    public CreateProcessingActivityCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Purpose).NotEmpty().MaximumLength(2000);
        RuleForEach(x => x.DataSubjectCategories)
            .Must(v => Enum.TryParse<DataSubjectCategory>(v, out _))
            .WithMessage("Each data subject category must be one of: " + string.Join(", ", Enum.GetNames<DataSubjectCategory>()));
    }
}

public sealed class CreateProcessingActivityCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateProcessingActivityCommand, ProcessingActivityDetailDto>
{
    public async Task<ProcessingActivityDetailDto> Handle(CreateProcessingActivityCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a processing activity.");
        }

        if (request.OwnerUserId is { } ownerId && !await db.Users.AnyAsync(u => u.Id == ownerId, cancellationToken))
        {
            throw new NotFoundException(nameof(User), ownerId);
        }

        if (request.RetentionPolicyId is { } policyId && !await db.RetentionPolicies.AnyAsync(p => p.Id == policyId, cancellationToken))
        {
            throw new NotFoundException(nameof(RetentionPolicy), policyId);
        }

        var activity = new ProcessingActivity
        {
            OrganisationId = organisationId,
            Name = request.Name.Trim(),
            Purpose = request.Purpose,
            DataSubjectCategoriesJson = DataInventoryMapper.SerializeJsonArray(request.DataSubjectCategories),
            SecurityControlsJson = DataInventoryMapper.SerializeJsonArray(request.SecurityControls),
            RetentionPolicyId = request.RetentionPolicyId,
            OwnerUserId = request.OwnerUserId,
            ReviewDate = request.ReviewDate,
            Status = ProcessingActivityStatus.DRAFT,
        };

        db.ProcessingActivities.Add(activity);
        await ProcessingActivityLoader.SetRelationshipsAsync(
            db, activity, request.DataCategoryIds, request.ItSystemIds, request.DataCollectionSourceIds, request.RecipientIds, request.ProcessorIds, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.activity_created", nameof(ProcessingActivity), activity.Id.ToString(), newValue: new { activity.Name }, cancellationToken: cancellationToken);

        var loaded = await ProcessingActivityLoader.LoadForDetailAsync(db, activity.Id, cancellationToken);
        return DataInventoryMapper.ToDetailDto(loaded);
    }
}

public sealed record UpdateProcessingActivityCommand(
    Guid Id, string Name, string Purpose, IReadOnlyList<string> DataSubjectCategories, IReadOnlyList<string> SecurityControls,
    Guid? RetentionPolicyId, Guid? OwnerUserId, DateOnly? ReviewDate,
    IReadOnlyList<Guid> DataCategoryIds, IReadOnlyList<Guid> ItSystemIds, IReadOnlyList<Guid> DataCollectionSourceIds,
    IReadOnlyList<Guid> RecipientIds, IReadOnlyList<Guid> ProcessorIds) : IRequest<ProcessingActivityDetailDto>;

public sealed class UpdateProcessingActivityCommandValidator : AbstractValidator<UpdateProcessingActivityCommand>
{
    public UpdateProcessingActivityCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Purpose).NotEmpty().MaximumLength(2000);
        RuleForEach(x => x.DataSubjectCategories)
            .Must(v => Enum.TryParse<DataSubjectCategory>(v, out _))
            .WithMessage("Each data subject category must be one of: " + string.Join(", ", Enum.GetNames<DataSubjectCategory>()));
    }
}

public sealed class UpdateProcessingActivityCommandHandler(IAppDbContext db, IAuditLogger auditLogger)
    : IRequestHandler<UpdateProcessingActivityCommand, ProcessingActivityDetailDto>
{
    public async Task<ProcessingActivityDetailDto> Handle(UpdateProcessingActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await ProcessingActivityLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (activity.Status == ProcessingActivityStatus.ARCHIVED)
        {
            throw new ConflictException("Cannot edit an archived processing activity — reopen it first is not supported; create a new one instead.");
        }

        if (request.OwnerUserId is { } ownerId && !await db.Users.AnyAsync(u => u.Id == ownerId, cancellationToken))
        {
            throw new NotFoundException(nameof(User), ownerId);
        }

        if (request.RetentionPolicyId is { } policyId && !await db.RetentionPolicies.AnyAsync(p => p.Id == policyId, cancellationToken))
        {
            throw new NotFoundException(nameof(RetentionPolicy), policyId);
        }

        activity.Name = request.Name.Trim();
        activity.Purpose = request.Purpose;
        activity.DataSubjectCategoriesJson = DataInventoryMapper.SerializeJsonArray(request.DataSubjectCategories);
        activity.SecurityControlsJson = DataInventoryMapper.SerializeJsonArray(request.SecurityControls);
        activity.RetentionPolicyId = request.RetentionPolicyId;
        activity.OwnerUserId = request.OwnerUserId;
        activity.ReviewDate = request.ReviewDate;

        await ProcessingActivityLoader.SetRelationshipsAsync(
            db, activity, request.DataCategoryIds, request.ItSystemIds, request.DataCollectionSourceIds, request.RecipientIds, request.ProcessorIds, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.activity_updated", nameof(ProcessingActivity), activity.Id.ToString(), cancellationToken: cancellationToken);

        var loaded = await ProcessingActivityLoader.LoadForDetailAsync(db, activity.Id, cancellationToken);
        return DataInventoryMapper.ToDetailDto(loaded);
    }
}

public sealed record SubmitProcessingActivityForReviewCommand(Guid Id) : IRequest<ProcessingActivityDetailDto>;

public sealed class SubmitProcessingActivityForReviewCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<SubmitProcessingActivityForReviewCommand, ProcessingActivityDetailDto>
{
    public async Task<ProcessingActivityDetailDto> Handle(SubmitProcessingActivityForReviewCommand request, CancellationToken cancellationToken)
    {
        var activity = await ProcessingActivityLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!ProcessingActivityStatusTransitions.CanTransition(activity.Status, ProcessingActivityStatus.IN_REVIEW))
        {
            throw new ConflictException($"Cannot submit a processing activity in {activity.Status} status for review.");
        }

        activity.Status = ProcessingActivityStatus.IN_REVIEW;
        activity.SubmittedForReviewAt = dateTimeProvider.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.activity_submitted_for_review", nameof(ProcessingActivity), activity.Id.ToString(), cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDetailDto(activity);
    }
}

public sealed record ApproveProcessingActivityCommand(Guid Id, string? ReviewComments) : IRequest<ProcessingActivityDetailDto>;

public sealed class ApproveProcessingActivityCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<ApproveProcessingActivityCommand, ProcessingActivityDetailDto>
{
    public async Task<ProcessingActivityDetailDto> Handle(ApproveProcessingActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await ProcessingActivityLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!ProcessingActivityStatusTransitions.CanTransition(activity.Status, ProcessingActivityStatus.APPROVED))
        {
            throw new ConflictException($"Cannot approve a processing activity in {activity.Status} status.");
        }

        var now = dateTimeProvider.UtcNow;
        activity.Status = ProcessingActivityStatus.APPROVED;
        activity.ReviewedAt = now;
        activity.ReviewedBy = currentUser.UserId;
        activity.ReviewComments = request.ReviewComments;
        activity.ApprovedAt = now;
        activity.ApprovedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.activity_approved", nameof(ProcessingActivity), activity.Id.ToString(), cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDetailDto(activity);
    }
}

public sealed record SendProcessingActivityBackToDraftCommand(Guid Id, string ReviewComments) : IRequest<ProcessingActivityDetailDto>;

public sealed class SendProcessingActivityBackToDraftCommandValidator : AbstractValidator<SendProcessingActivityBackToDraftCommand>
{
    public SendProcessingActivityBackToDraftCommandValidator()
    {
        RuleFor(x => x.ReviewComments).NotEmpty().MaximumLength(2000);
    }
}

public sealed class SendProcessingActivityBackToDraftCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<SendProcessingActivityBackToDraftCommand, ProcessingActivityDetailDto>
{
    public async Task<ProcessingActivityDetailDto> Handle(SendProcessingActivityBackToDraftCommand request, CancellationToken cancellationToken)
    {
        var activity = await ProcessingActivityLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (activity.Status != ProcessingActivityStatus.IN_REVIEW)
        {
            throw new ConflictException($"Cannot send a processing activity in {activity.Status} status back to draft — only a submitted (IN_REVIEW) activity can be rejected. Use reopen for an already-approved activity.");
        }

        activity.Status = ProcessingActivityStatus.DRAFT;
        activity.ReviewedAt = dateTimeProvider.UtcNow;
        activity.ReviewedBy = currentUser.UserId;
        activity.ReviewComments = request.ReviewComments;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.activity_sent_back_to_draft", nameof(ProcessingActivity), activity.Id.ToString(), newValue: new { request.ReviewComments }, cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDetailDto(activity);
    }
}

public sealed record ArchiveProcessingActivityCommand(Guid Id) : IRequest<ProcessingActivityDetailDto>;

public sealed class ArchiveProcessingActivityCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<ArchiveProcessingActivityCommand, ProcessingActivityDetailDto>
{
    public async Task<ProcessingActivityDetailDto> Handle(ArchiveProcessingActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await ProcessingActivityLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!ProcessingActivityStatusTransitions.CanTransition(activity.Status, ProcessingActivityStatus.ARCHIVED))
        {
            throw new ConflictException($"Cannot archive a processing activity in {activity.Status} status.");
        }

        activity.Status = ProcessingActivityStatus.ARCHIVED;
        activity.ArchivedAt = dateTimeProvider.UtcNow;
        activity.ArchivedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.activity_archived", nameof(ProcessingActivity), activity.Id.ToString(), cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDetailDto(activity);
    }
}

public sealed record ReopenProcessingActivityCommand(Guid Id) : IRequest<ProcessingActivityDetailDto>;

public sealed class ReopenProcessingActivityCommandHandler(IAppDbContext db, IAuditLogger auditLogger)
    : IRequestHandler<ReopenProcessingActivityCommand, ProcessingActivityDetailDto>
{
    public async Task<ProcessingActivityDetailDto> Handle(ReopenProcessingActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await ProcessingActivityLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (activity.Status != ProcessingActivityStatus.APPROVED)
        {
            throw new ConflictException($"Cannot reopen a processing activity in {activity.Status} status — only an APPROVED activity can be reopened for edits.");
        }

        activity.Status = ProcessingActivityStatus.DRAFT;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.activity_reopened", nameof(ProcessingActivity), activity.Id.ToString(), cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDetailDto(activity);
    }
}

public sealed record DeleteProcessingActivityCommand(Guid Id) : IRequest;

public sealed class DeleteProcessingActivityCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteProcessingActivityCommand>
{
    public async Task Handle(DeleteProcessingActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await ProcessingActivityLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        activity.IsDeleted = true;
        activity.DeletedAt = dateTimeProvider.UtcNow;
        activity.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.activity_deleted", nameof(ProcessingActivity), activity.Id.ToString(), cancellationToken: cancellationToken);
    }
}
