using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Domain.Modules.ConsentPrivacy;
using DPDP.Domain.Modules.DataInventory;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.ConsentPrivacy.Commands;

internal static class DataPrincipalLoader
{
    public static async Task<DataPrincipal> LoadAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.DataPrincipals.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(DataPrincipal), id);
}

public sealed record CreateDataPrincipalCommand(string ExternalReferenceId, string? ReferenceCategory, string? Notes) : IRequest<DataPrincipalDto>;

public sealed class CreateDataPrincipalCommandValidator : AbstractValidator<CreateDataPrincipalCommand>
{
    public CreateDataPrincipalCommandValidator()
    {
        RuleFor(x => x.ExternalReferenceId).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.ReferenceCategory)
            .Must(v => v is null || Enum.TryParse<DataSubjectCategory>(v, out _))
            .WithMessage("referenceCategory must be one of: " + string.Join(", ", Enum.GetNames<DataSubjectCategory>()));
    }
}

public sealed class CreateDataPrincipalCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateDataPrincipalCommand, DataPrincipalDto>
{
    public async Task<DataPrincipalDto> Handle(CreateDataPrincipalCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a data principal reference.");
        }

        var principal = new DataPrincipal
        {
            OrganisationId = organisationId,
            ExternalReferenceId = request.ExternalReferenceId.Trim(),
            ReferenceCategory = request.ReferenceCategory is null ? null : Enum.Parse<DataSubjectCategory>(request.ReferenceCategory),
            Notes = request.Notes,
        };

        db.DataPrincipals.Add(principal);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.principal_created", nameof(DataPrincipal), principal.Id.ToString(), cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDto(principal);
    }
}

public sealed record UpdateDataPrincipalCommand(Guid Id, string ExternalReferenceId, string? ReferenceCategory, string? Notes, bool IsActive) : IRequest<DataPrincipalDto>;

public sealed class UpdateDataPrincipalCommandValidator : AbstractValidator<UpdateDataPrincipalCommand>
{
    public UpdateDataPrincipalCommandValidator()
    {
        RuleFor(x => x.ExternalReferenceId).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.ReferenceCategory)
            .Must(v => v is null || Enum.TryParse<DataSubjectCategory>(v, out _))
            .WithMessage("referenceCategory must be one of: " + string.Join(", ", Enum.GetNames<DataSubjectCategory>()));
    }
}

public sealed class UpdateDataPrincipalCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<UpdateDataPrincipalCommand, DataPrincipalDto>
{
    public async Task<DataPrincipalDto> Handle(UpdateDataPrincipalCommand request, CancellationToken cancellationToken)
    {
        var principal = await DataPrincipalLoader.LoadAsync(db, request.Id, cancellationToken);

        principal.ExternalReferenceId = request.ExternalReferenceId.Trim();
        principal.ReferenceCategory = request.ReferenceCategory is null ? null : Enum.Parse<DataSubjectCategory>(request.ReferenceCategory);
        principal.Notes = request.Notes;
        principal.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.principal_updated", nameof(DataPrincipal), principal.Id.ToString(), cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDto(principal);
    }
}

public sealed record DeleteDataPrincipalCommand(Guid Id) : IRequest;

public sealed class DeleteDataPrincipalCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteDataPrincipalCommand>
{
    public async Task Handle(DeleteDataPrincipalCommand request, CancellationToken cancellationToken)
    {
        var principal = await DataPrincipalLoader.LoadAsync(db, request.Id, cancellationToken);

        var inUseByConsent = await db.ConsentRecords.AnyAsync(c => c.DataPrincipalId == principal.Id, cancellationToken);
        var inUseByRequest = await db.DataPrincipalRequests.AnyAsync(r => r.DataPrincipalId == principal.Id, cancellationToken);
        if (inUseByConsent || inUseByRequest)
        {
            throw new ConflictException("Cannot remove a data principal reference that still has consent records or requests linked to it.");
        }

        principal.IsDeleted = true;
        principal.DeletedAt = dateTimeProvider.UtcNow;
        principal.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.principal_deleted", nameof(DataPrincipal), principal.Id.ToString(), cancellationToken: cancellationToken);
    }
}
