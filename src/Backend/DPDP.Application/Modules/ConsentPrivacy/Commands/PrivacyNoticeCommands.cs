using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Domain.Modules.ConsentPrivacy;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.ConsentPrivacy.Commands;

internal static class PrivacyNoticeLoader
{
    public static async Task<PrivacyNotice> LoadForDetailAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.PrivacyNotices.Include(n => n.DataCategories).FirstOrDefaultAsync(n => n.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(PrivacyNotice), id);
}

public sealed record CreatePrivacyNoticeCommand(
    string Code, string Title, string Version, string Language, string Purpose,
    DateOnly? PublishedDate, DateOnly? EffectiveDate, IReadOnlyList<Guid> DataCategoryIds) : IRequest<PrivacyNoticeDetailDto>;

public sealed class CreatePrivacyNoticeCommandValidator : AbstractValidator<CreatePrivacyNoticeCommand>
{
    public CreatePrivacyNoticeCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Version).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Language).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Purpose).NotEmpty().MaximumLength(2000);
    }
}

public sealed class CreatePrivacyNoticeCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreatePrivacyNoticeCommand, PrivacyNoticeDetailDto>
{
    public async Task<PrivacyNoticeDetailDto> Handle(CreatePrivacyNoticeCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a privacy notice.");
        }

        if (await db.PrivacyNotices.AnyAsync(n => n.Code == request.Code && n.Version == request.Version, cancellationToken))
        {
            throw new ConflictException($"A privacy notice with code '{request.Code}' and version '{request.Version}' already exists.");
        }

        var notice = new PrivacyNotice
        {
            OrganisationId = organisationId,
            Code = request.Code.Trim(),
            Title = request.Title.Trim(),
            Version = request.Version.Trim(),
            Language = request.Language.Trim(),
            Purpose = request.Purpose,
            PublishedDate = request.PublishedDate,
            EffectiveDate = request.EffectiveDate,
            Status = PrivacyNoticeStatus.DRAFT,
        };

        foreach (var categoryId in request.DataCategoryIds.Distinct())
        {
            var category = await db.DataCategories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken)
                ?? throw new NotFoundException("DataCategory", categoryId);
            notice.DataCategories.Add(category);
        }

        db.PrivacyNotices.Add(notice);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.notice_created", nameof(PrivacyNotice), notice.Id.ToString(), newValue: new { notice.Code, notice.Version }, cancellationToken: cancellationToken);

        var loaded = await PrivacyNoticeLoader.LoadForDetailAsync(db, notice.Id, cancellationToken);
        return ConsentPrivacyMapper.ToDetailDto(loaded);
    }
}

public sealed record UpdatePrivacyNoticeCommand(
    Guid Id, string Title, string Language, string Purpose,
    DateOnly? PublishedDate, DateOnly? EffectiveDate, IReadOnlyList<Guid> DataCategoryIds) : IRequest<PrivacyNoticeDetailDto>;

public sealed class UpdatePrivacyNoticeCommandValidator : AbstractValidator<UpdatePrivacyNoticeCommand>
{
    public UpdatePrivacyNoticeCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Language).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Purpose).NotEmpty().MaximumLength(2000);
    }
}

public sealed class UpdatePrivacyNoticeCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<UpdatePrivacyNoticeCommand, PrivacyNoticeDetailDto>
{
    public async Task<PrivacyNoticeDetailDto> Handle(UpdatePrivacyNoticeCommand request, CancellationToken cancellationToken)
    {
        var notice = await PrivacyNoticeLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (notice.Status != PrivacyNoticeStatus.DRAFT)
        {
            throw new ConflictException($"Cannot edit a privacy notice in {notice.Status} status — only DRAFT notices can be edited. Create a new version instead.");
        }

        notice.Title = request.Title.Trim();
        notice.Language = request.Language.Trim();
        notice.Purpose = request.Purpose;
        notice.PublishedDate = request.PublishedDate;
        notice.EffectiveDate = request.EffectiveDate;

        notice.DataCategories.Clear();
        foreach (var categoryId in request.DataCategoryIds.Distinct())
        {
            var category = await db.DataCategories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken)
                ?? throw new NotFoundException("DataCategory", categoryId);
            notice.DataCategories.Add(category);
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.notice_updated", nameof(PrivacyNotice), notice.Id.ToString(), cancellationToken: cancellationToken);

        var loaded = await PrivacyNoticeLoader.LoadForDetailAsync(db, notice.Id, cancellationToken);
        return ConsentPrivacyMapper.ToDetailDto(loaded);
    }
}

public sealed record ApprovePrivacyNoticeCommand(Guid Id) : IRequest<PrivacyNoticeDetailDto>;

public sealed class ApprovePrivacyNoticeCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<ApprovePrivacyNoticeCommand, PrivacyNoticeDetailDto>
{
    public async Task<PrivacyNoticeDetailDto> Handle(ApprovePrivacyNoticeCommand request, CancellationToken cancellationToken)
    {
        var notice = await PrivacyNoticeLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!PrivacyNoticeStatusTransitions.CanTransition(notice.Status, PrivacyNoticeStatus.APPROVED))
        {
            throw new ConflictException($"Cannot approve a privacy notice in {notice.Status} status.");
        }

        notice.Status = PrivacyNoticeStatus.APPROVED;
        notice.ApprovedAt = dateTimeProvider.UtcNow;
        notice.ApprovedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.notice_approved", nameof(PrivacyNotice), notice.Id.ToString(), cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDetailDto(notice);
    }
}

public sealed record PublishPrivacyNoticeCommand(Guid Id) : IRequest<PrivacyNoticeDetailDto>;

public sealed class PublishPrivacyNoticeCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<PublishPrivacyNoticeCommand, PrivacyNoticeDetailDto>
{
    public async Task<PrivacyNoticeDetailDto> Handle(PublishPrivacyNoticeCommand request, CancellationToken cancellationToken)
    {
        var notice = await PrivacyNoticeLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!PrivacyNoticeStatusTransitions.CanTransition(notice.Status, PrivacyNoticeStatus.PUBLISHED))
        {
            throw new ConflictException($"Cannot publish a privacy notice in {notice.Status} status — it must be APPROVED first.");
        }

        notice.Status = PrivacyNoticeStatus.PUBLISHED;
        notice.PublishedAt = dateTimeProvider.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.notice_published", nameof(PrivacyNotice), notice.Id.ToString(), cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDetailDto(notice);
    }
}

public sealed record ArchivePrivacyNoticeCommand(Guid Id) : IRequest<PrivacyNoticeDetailDto>;

public sealed class ArchivePrivacyNoticeCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<ArchivePrivacyNoticeCommand, PrivacyNoticeDetailDto>
{
    public async Task<PrivacyNoticeDetailDto> Handle(ArchivePrivacyNoticeCommand request, CancellationToken cancellationToken)
    {
        var notice = await PrivacyNoticeLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!PrivacyNoticeStatusTransitions.CanTransition(notice.Status, PrivacyNoticeStatus.ARCHIVED))
        {
            throw new ConflictException($"Cannot archive a privacy notice in {notice.Status} status — only a PUBLISHED notice can be archived.");
        }

        notice.Status = PrivacyNoticeStatus.ARCHIVED;
        notice.ArchivedAt = dateTimeProvider.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.notice_archived", nameof(PrivacyNotice), notice.Id.ToString(), cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDetailDto(notice);
    }
}

public sealed record DeletePrivacyNoticeCommand(Guid Id) : IRequest;

public sealed class DeletePrivacyNoticeCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeletePrivacyNoticeCommand>
{
    public async Task Handle(DeletePrivacyNoticeCommand request, CancellationToken cancellationToken)
    {
        var notice = await PrivacyNoticeLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (notice.Status != PrivacyNoticeStatus.DRAFT)
        {
            throw new ConflictException($"Cannot remove a privacy notice in {notice.Status} status — only a DRAFT notice can be removed; archive a published one instead.");
        }

        notice.IsDeleted = true;
        notice.DeletedAt = dateTimeProvider.UtcNow;
        notice.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.notice_deleted", nameof(PrivacyNotice), notice.Id.ToString(), cancellationToken: cancellationToken);
    }
}
