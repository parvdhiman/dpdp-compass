using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Domain.Modules.ConsentPrivacy;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.ConsentPrivacy.Commands;

internal static class ConsentRecordLoader
{
    public static async Task<ConsentRecord> LoadForDetailAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.ConsentRecords
            .Include(c => c.DataPrincipal)
            .Include(c => c.ConsentPurpose)
            .Include(c => c.NoticeVersion)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(ConsentRecord), id);
}

public sealed record CreateConsentCommand(
    Guid DataPrincipalId, Guid ConsentPurposeId, Guid? NoticeVersionId, DateTimeOffset? GrantedAt,
    string Channel, DateTimeOffset? ExpiresAt, string? SourceSystem, string? ExternalReferenceId) : IRequest<ConsentRecordDto>;

public sealed class CreateConsentCommandValidator : AbstractValidator<CreateConsentCommand>
{
    public CreateConsentCommandValidator()
    {
        RuleFor(x => x.DataPrincipalId).NotEmpty();
        RuleFor(x => x.ConsentPurposeId).NotEmpty();
        RuleFor(x => x.Channel).Must(v => Enum.TryParse<ConsentChannel>(v, out _)).WithMessage("channel must be one of: " + string.Join(", ", Enum.GetNames<ConsentChannel>()));
        RuleFor(x => x.SourceSystem).MaximumLength(200);
        RuleFor(x => x.ExternalReferenceId).MaximumLength(300);
    }
}

public sealed class CreateConsentCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<CreateConsentCommand, ConsentRecordDto>
{
    public async Task<ConsentRecordDto> Handle(CreateConsentCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can capture consent.");
        }

        if (!await db.DataPrincipals.AnyAsync(p => p.Id == request.DataPrincipalId, cancellationToken))
        {
            throw new NotFoundException(nameof(DataPrincipal), request.DataPrincipalId);
        }

        if (!await db.ConsentPurposes.AnyAsync(p => p.Id == request.ConsentPurposeId, cancellationToken))
        {
            throw new NotFoundException(nameof(ConsentPurpose), request.ConsentPurposeId);
        }

        if (request.NoticeVersionId is { } noticeId && !await db.PrivacyNotices.AnyAsync(n => n.Id == noticeId, cancellationToken))
        {
            throw new NotFoundException(nameof(PrivacyNotice), noticeId);
        }

        var consent = new ConsentRecord
        {
            OrganisationId = organisationId,
            DataPrincipalId = request.DataPrincipalId,
            ConsentPurposeId = request.ConsentPurposeId,
            NoticeVersionId = request.NoticeVersionId,
            GrantedAt = request.GrantedAt ?? dateTimeProvider.UtcNow,
            Channel = Enum.Parse<ConsentChannel>(request.Channel),
            Status = ConsentStatus.GRANTED,
            ExpiresAt = request.ExpiresAt,
            SourceSystem = request.SourceSystem,
            ExternalReferenceId = request.ExternalReferenceId,
        };

        db.ConsentRecords.Add(consent);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.consent_granted", nameof(ConsentRecord), consent.Id.ToString(), newValue: new { consent.ConsentPurposeId, consent.Channel }, cancellationToken: cancellationToken);

        var loaded = await ConsentRecordLoader.LoadForDetailAsync(db, consent.Id, cancellationToken);
        return ConsentPrivacyMapper.ToDto(loaded);
    }
}

public sealed record WithdrawConsentCommand(Guid Id) : IRequest<ConsentRecordDto>;

public sealed class WithdrawConsentCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<WithdrawConsentCommand, ConsentRecordDto>
{
    public async Task<ConsentRecordDto> Handle(WithdrawConsentCommand request, CancellationToken cancellationToken)
    {
        var consent = await ConsentRecordLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!ConsentStatusTransitions.CanTransition(consent.Status, ConsentStatus.WITHDRAWN))
        {
            throw new ConflictException($"Cannot withdraw a consent record in {consent.Status} status.");
        }

        consent.Status = ConsentStatus.WITHDRAWN;
        consent.WithdrawnAt = dateTimeProvider.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.consent_withdrawn", nameof(ConsentRecord), consent.Id.ToString(), cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDto(consent);
    }
}

/// <summary>Organisation-initiated invalidation (e.g. a notice was retracted, a legal issue) — distinct from WithdrawConsentCommand, which represents the data principal's own action. See docs/CONSENT_PRIVACY_OPERATIONS.md section 3.</summary>
public sealed record RevokeConsentCommand(Guid Id, string Reason) : IRequest<ConsentRecordDto>;

public sealed class RevokeConsentCommandValidator : AbstractValidator<RevokeConsentCommand>
{
    public RevokeConsentCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
    }
}

public sealed class RevokeConsentCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<RevokeConsentCommand, ConsentRecordDto>
{
    public async Task<ConsentRecordDto> Handle(RevokeConsentCommand request, CancellationToken cancellationToken)
    {
        var consent = await ConsentRecordLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!ConsentStatusTransitions.CanTransition(consent.Status, ConsentStatus.REVOKED))
        {
            throw new ConflictException($"Cannot revoke a consent record in {consent.Status} status.");
        }

        consent.Status = ConsentStatus.REVOKED;
        consent.WithdrawnAt = dateTimeProvider.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.consent_revoked", nameof(ConsentRecord), consent.Id.ToString(), newValue: new { request.Reason }, cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDto(consent);
    }
}

/// <summary>Batch-transitions GRANTED consents whose ExpiresAt has passed to EXPIRED — the same on-demand stand-in for a scheduler as Module 7's MarkEvidenceExpiredCommand and Module 8's MarkEvidenceExpiredCommand pattern.</summary>
public sealed record MarkConsentExpiredCommand : IRequest<int>;

public sealed class MarkConsentExpiredCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<MarkConsentExpiredCommand, int>
{
    public async Task<int> Handle(MarkConsentExpiredCommand request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;

        var expired = await db.ConsentRecords
            .Where(c => c.Status == ConsentStatus.GRANTED && c.ExpiresAt != null && c.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        foreach (var consent in expired)
        {
            consent.Status = ConsentStatus.EXPIRED;
            await auditLogger.LogAsync("consentprivacy.consent_expired", nameof(ConsentRecord), consent.Id.ToString(), cancellationToken: cancellationToken);
        }

        if (expired.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return expired.Count;
    }
}
