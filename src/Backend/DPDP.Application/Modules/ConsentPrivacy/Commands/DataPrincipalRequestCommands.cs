using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Domain.Modules.ConsentPrivacy;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.ConsentPrivacy.Commands;

internal static class DataPrincipalRequestLoader
{
    public static async Task<DataPrincipalRequest> LoadForDetailAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.DataPrincipalRequests
            .Include(r => r.DataPrincipal)
            .Include(r => r.RelatedConsent)
            .Include(r => r.SlaPolicy)
            .Include(r => r.AssignedToUser)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(DataPrincipalRequest), id);
}

/// <summary>
/// The ONLY place a Data Principal Request's due date is computed — never
/// a literal day-count. Picks the most specific active policy configured
/// for this organisation: one scoped to this exact RequestType first,
/// falling back to a catch-all (RequestType == null) policy, and finally
/// to "no due date at all" if nothing is configured — see the brief's
/// "do not hard-code legal deadlines without verified legal source" and
/// docs/CONSENT_PRIVACY_OPERATIONS.md section 5.
/// </summary>
internal static class SlaPolicyResolver
{
    public static async Task<SlaPolicy?> ResolveAsync(IAppDbContext db, DataPrincipalRequestType requestType, CancellationToken cancellationToken)
    {
        var specific = await db.SlaPolicies.FirstOrDefaultAsync(p => p.IsActive && p.RequestType == requestType, cancellationToken);
        if (specific is not null)
        {
            return specific;
        }

        return await db.SlaPolicies.FirstOrDefaultAsync(p => p.IsActive && p.RequestType == null, cancellationToken);
    }
}

public sealed record CreateDataPrincipalRequestCommand(
    string RequestType, string RequesterName, string? RequesterContactEmail, string? RequesterContactPhone,
    string? ExternalReferenceId, Guid? DataPrincipalId, Guid? RelatedConsentId, string? Description) : IRequest<DataPrincipalRequestDetailDto>;

public sealed class CreateDataPrincipalRequestCommandValidator : AbstractValidator<CreateDataPrincipalRequestCommand>
{
    public CreateDataPrincipalRequestCommandValidator()
    {
        RuleFor(x => x.RequestType).Must(v => Enum.TryParse<DataPrincipalRequestType>(v, out _)).WithMessage("requestType must be one of: " + string.Join(", ", Enum.GetNames<DataPrincipalRequestType>()));
        RuleFor(x => x.RequesterName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.RequesterContactEmail).EmailAddress().MaximumLength(300).When(x => !string.IsNullOrEmpty(x.RequesterContactEmail));
        RuleFor(x => x.RequesterContactPhone).MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(4000);
    }
}

public sealed class CreateDataPrincipalRequestCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<CreateDataPrincipalRequestCommand, DataPrincipalRequestDetailDto>
{
    public async Task<DataPrincipalRequestDetailDto> Handle(CreateDataPrincipalRequestCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can log a data principal request.");
        }

        if (request.DataPrincipalId is { } principalId && !await db.DataPrincipals.AnyAsync(p => p.Id == principalId, cancellationToken))
        {
            throw new NotFoundException(nameof(DataPrincipal), principalId);
        }

        if (request.RelatedConsentId is { } consentId && !await db.ConsentRecords.AnyAsync(c => c.Id == consentId, cancellationToken))
        {
            throw new NotFoundException(nameof(ConsentRecord), consentId);
        }

        var requestType = Enum.Parse<DataPrincipalRequestType>(request.RequestType);
        var slaPolicy = await SlaPolicyResolver.ResolveAsync(db, requestType, cancellationToken);
        var now = dateTimeProvider.UtcNow;

        var dprRequest = new DataPrincipalRequest
        {
            OrganisationId = organisationId,
            RequestType = requestType,
            Status = DataPrincipalRequestStatus.REQUESTED,
            RequesterName = request.RequesterName.Trim(),
            RequesterContactEmail = request.RequesterContactEmail,
            RequesterContactPhone = request.RequesterContactPhone,
            ExternalReferenceId = request.ExternalReferenceId,
            DataPrincipalId = request.DataPrincipalId,
            RelatedConsentId = request.RelatedConsentId,
            Description = request.Description,
            SlaPolicyId = slaPolicy?.Id,
            DueAt = slaPolicy is null ? null : now.AddDays(slaPolicy.ResponseDueDays),
        };

        db.DataPrincipalRequests.Add(dprRequest);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "consentprivacy.request_created", nameof(DataPrincipalRequest), dprRequest.Id.ToString(),
            newValue: new { dprRequest.RequestType, SlaPolicyApplied = slaPolicy?.Name, dprRequest.DueAt }, cancellationToken: cancellationToken);

        var loaded = await DataPrincipalRequestLoader.LoadForDetailAsync(db, dprRequest.Id, cancellationToken);
        return ConsentPrivacyMapper.ToDetailDto(loaded, dateTimeProvider);
    }
}

public sealed record AssignDataPrincipalRequestCommand(Guid Id, Guid AssignedToUserId) : IRequest<DataPrincipalRequestDetailDto>;

public sealed class AssignDataPrincipalRequestCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<AssignDataPrincipalRequestCommand, DataPrincipalRequestDetailDto>
{
    public async Task<DataPrincipalRequestDetailDto> Handle(AssignDataPrincipalRequestCommand request, CancellationToken cancellationToken)
    {
        var dprRequest = await DataPrincipalRequestLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.AssignedToUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.AssignedToUserId);

        dprRequest.AssignedToUserId = user.Id;
        dprRequest.AssignedToUser = user;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.request_assigned", nameof(DataPrincipalRequest), dprRequest.Id.ToString(), newValue: new { user.FullName }, cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDetailDto(dprRequest, dateTimeProvider);
    }
}

public sealed record VerifyDataPrincipalRequestIdentityCommand(Guid Id, Guid? MatchedDataPrincipalId) : IRequest<DataPrincipalRequestDetailDto>;

public sealed class VerifyDataPrincipalRequestIdentityCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<VerifyDataPrincipalRequestIdentityCommand, DataPrincipalRequestDetailDto>
{
    public async Task<DataPrincipalRequestDetailDto> Handle(VerifyDataPrincipalRequestIdentityCommand request, CancellationToken cancellationToken)
    {
        var dprRequest = await DataPrincipalRequestLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (dprRequest.Status != DataPrincipalRequestStatus.REQUESTED && dprRequest.Status != DataPrincipalRequestStatus.IDENTITY_VERIFICATION)
        {
            throw new ConflictException($"Cannot verify identity for a request in {dprRequest.Status} status.");
        }

        if (request.MatchedDataPrincipalId is { } principalId)
        {
            if (!await db.DataPrincipals.AnyAsync(p => p.Id == principalId, cancellationToken))
            {
                throw new NotFoundException(nameof(DataPrincipal), principalId);
            }

            dprRequest.DataPrincipalId = principalId;
        }

        if (dprRequest.Status == DataPrincipalRequestStatus.REQUESTED)
        {
            dprRequest.Status = DataPrincipalRequestStatus.IDENTITY_VERIFICATION;
        }

        dprRequest.IdentityVerifiedAt = dateTimeProvider.UtcNow;
        dprRequest.IdentityVerifiedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.request_identity_verified", nameof(DataPrincipalRequest), dprRequest.Id.ToString(), cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDetailDto(dprRequest, dateTimeProvider);
    }
}

/// <summary>General progress transitions only — REQUESTED→IDENTITY_VERIFICATION is done by VerifyDataPrincipalRequestIdentityCommand; COMPLETED/REJECTED/CLOSED go through their own dedicated commands (closing-authority actions), the same shape as Module 6's UpdateFindingStatusCommand.</summary>
public sealed record UpdateDataPrincipalRequestStatusCommand(Guid Id, string Status) : IRequest<DataPrincipalRequestDetailDto>;

public sealed class UpdateDataPrincipalRequestStatusCommandValidator : AbstractValidator<UpdateDataPrincipalRequestStatusCommand>
{
    public UpdateDataPrincipalRequestStatusCommandValidator()
    {
        RuleFor(x => x.Status)
            .Must(v => Enum.TryParse<DataPrincipalRequestStatus>(v, out var s) && s is DataPrincipalRequestStatus.IN_PROGRESS or DataPrincipalRequestStatus.AWAITING_INFORMATION)
            .WithMessage("status must be one of: IN_PROGRESS, AWAITING_INFORMATION (use verify-identity/complete/reject/close for the others)");
    }
}

public sealed class UpdateDataPrincipalRequestStatusCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<UpdateDataPrincipalRequestStatusCommand, DataPrincipalRequestDetailDto>
{
    public async Task<DataPrincipalRequestDetailDto> Handle(UpdateDataPrincipalRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var dprRequest = await DataPrincipalRequestLoader.LoadForDetailAsync(db, request.Id, cancellationToken);
        var newStatus = Enum.Parse<DataPrincipalRequestStatus>(request.Status);

        if (!DataPrincipalRequestStatusTransitions.CanTransition(dprRequest.Status, newStatus))
        {
            throw new ConflictException($"Cannot move a request from {dprRequest.Status} to {newStatus}.");
        }

        var oldStatus = dprRequest.Status;
        dprRequest.Status = newStatus;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "consentprivacy.request_status_changed", nameof(DataPrincipalRequest), dprRequest.Id.ToString(),
            new { Status = oldStatus.ToString() }, new { Status = newStatus.ToString() }, cancellationToken);

        return ConsentPrivacyMapper.ToDetailDto(dprRequest, dateTimeProvider);
    }
}

public sealed record CompleteDataPrincipalRequestCommand(Guid Id, string? ResolutionNotes) : IRequest<DataPrincipalRequestDetailDto>;

public sealed class CompleteDataPrincipalRequestCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<CompleteDataPrincipalRequestCommand, DataPrincipalRequestDetailDto>
{
    public async Task<DataPrincipalRequestDetailDto> Handle(CompleteDataPrincipalRequestCommand request, CancellationToken cancellationToken)
    {
        var dprRequest = await DataPrincipalRequestLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!DataPrincipalRequestStatusTransitions.CanTransition(dprRequest.Status, DataPrincipalRequestStatus.COMPLETED))
        {
            throw new ConflictException($"Cannot complete a request in {dprRequest.Status} status.");
        }

        dprRequest.Status = DataPrincipalRequestStatus.COMPLETED;
        dprRequest.ResolvedAt = dateTimeProvider.UtcNow;
        dprRequest.ResolutionNotes = request.ResolutionNotes;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.request_completed", nameof(DataPrincipalRequest), dprRequest.Id.ToString(), cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDetailDto(dprRequest, dateTimeProvider);
    }
}

public sealed record RejectDataPrincipalRequestCommand(Guid Id, string RejectionReason) : IRequest<DataPrincipalRequestDetailDto>;

public sealed class RejectDataPrincipalRequestCommandValidator : AbstractValidator<RejectDataPrincipalRequestCommand>
{
    public RejectDataPrincipalRequestCommandValidator()
    {
        RuleFor(x => x.RejectionReason).NotEmpty().MaximumLength(2000);
    }
}

public sealed class RejectDataPrincipalRequestCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<RejectDataPrincipalRequestCommand, DataPrincipalRequestDetailDto>
{
    public async Task<DataPrincipalRequestDetailDto> Handle(RejectDataPrincipalRequestCommand request, CancellationToken cancellationToken)
    {
        var dprRequest = await DataPrincipalRequestLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!DataPrincipalRequestStatusTransitions.CanTransition(dprRequest.Status, DataPrincipalRequestStatus.REJECTED))
        {
            throw new ConflictException($"Cannot reject a request in {dprRequest.Status} status.");
        }

        dprRequest.Status = DataPrincipalRequestStatus.REJECTED;
        dprRequest.RejectedAt = dateTimeProvider.UtcNow;
        dprRequest.RejectionReason = request.RejectionReason;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "consentprivacy.request_rejected", nameof(DataPrincipalRequest), dprRequest.Id.ToString(),
            newValue: new { request.RejectionReason }, cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDetailDto(dprRequest, dateTimeProvider);
    }
}

public sealed record CloseDataPrincipalRequestCommand(Guid Id) : IRequest<DataPrincipalRequestDetailDto>;

public sealed class CloseDataPrincipalRequestCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<CloseDataPrincipalRequestCommand, DataPrincipalRequestDetailDto>
{
    public async Task<DataPrincipalRequestDetailDto> Handle(CloseDataPrincipalRequestCommand request, CancellationToken cancellationToken)
    {
        var dprRequest = await DataPrincipalRequestLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!DataPrincipalRequestStatusTransitions.CanTransition(dprRequest.Status, DataPrincipalRequestStatus.CLOSED))
        {
            throw new ConflictException($"Cannot close a request in {dprRequest.Status} status — it must be COMPLETED or REJECTED first.");
        }

        dprRequest.Status = DataPrincipalRequestStatus.CLOSED;
        dprRequest.ClosedAt = dateTimeProvider.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.request_closed", nameof(DataPrincipalRequest), dprRequest.Id.ToString(), cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDetailDto(dprRequest, dateTimeProvider);
    }
}

public sealed record DeleteDataPrincipalRequestCommand(Guid Id) : IRequest;

public sealed class DeleteDataPrincipalRequestCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteDataPrincipalRequestCommand>
{
    public async Task Handle(DeleteDataPrincipalRequestCommand request, CancellationToken cancellationToken)
    {
        var dprRequest = await DataPrincipalRequestLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        dprRequest.IsDeleted = true;
        dprRequest.DeletedAt = dateTimeProvider.UtcNow;
        dprRequest.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.request_deleted", nameof(DataPrincipalRequest), dprRequest.Id.ToString(), cancellationToken: cancellationToken);
    }
}
