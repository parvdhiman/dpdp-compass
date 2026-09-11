using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Domain.Modules.ConsentPrivacy;
using DPDP.Domain.Modules.DataInventory;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.ConsentPrivacy.Commands;

internal static class ConsentPurposeLoader
{
    public static async Task<ConsentPurpose> LoadAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.ConsentPurposes.Include(p => p.DataCategory).FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(ConsentPurpose), id);
}

public sealed record CreateConsentPurposeCommand(string Name, string? Description, Guid? DataCategoryId) : IRequest<ConsentPurposeDto>;

public sealed class CreateConsentPurposeCommandValidator : AbstractValidator<CreateConsentPurposeCommand>
{
    public CreateConsentPurposeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class CreateConsentPurposeCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateConsentPurposeCommand, ConsentPurposeDto>
{
    public async Task<ConsentPurposeDto> Handle(CreateConsentPurposeCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a consent purpose.");
        }

        if (request.DataCategoryId is { } categoryId && !await db.DataCategories.AnyAsync(c => c.Id == categoryId, cancellationToken))
        {
            throw new NotFoundException(nameof(DataCategory), categoryId);
        }

        var purpose = new ConsentPurpose
        {
            OrganisationId = organisationId,
            Name = request.Name.Trim(),
            Description = request.Description,
            DataCategoryId = request.DataCategoryId,
        };

        db.ConsentPurposes.Add(purpose);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.purpose_created", nameof(ConsentPurpose), purpose.Id.ToString(), newValue: new { purpose.Name }, cancellationToken: cancellationToken);

        var loaded = await ConsentPurposeLoader.LoadAsync(db, purpose.Id, cancellationToken);
        return ConsentPrivacyMapper.ToDto(loaded);
    }
}

public sealed record UpdateConsentPurposeCommand(Guid Id, string Name, string? Description, Guid? DataCategoryId, bool IsActive) : IRequest<ConsentPurposeDto>;

public sealed class UpdateConsentPurposeCommandValidator : AbstractValidator<UpdateConsentPurposeCommand>
{
    public UpdateConsentPurposeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class UpdateConsentPurposeCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<UpdateConsentPurposeCommand, ConsentPurposeDto>
{
    public async Task<ConsentPurposeDto> Handle(UpdateConsentPurposeCommand request, CancellationToken cancellationToken)
    {
        var purpose = await ConsentPurposeLoader.LoadAsync(db, request.Id, cancellationToken);

        if (request.DataCategoryId is { } categoryId && !await db.DataCategories.AnyAsync(c => c.Id == categoryId, cancellationToken))
        {
            throw new NotFoundException(nameof(DataCategory), categoryId);
        }

        purpose.Name = request.Name.Trim();
        purpose.Description = request.Description;
        purpose.DataCategoryId = request.DataCategoryId;
        purpose.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.purpose_updated", nameof(ConsentPurpose), purpose.Id.ToString(), cancellationToken: cancellationToken);

        var loaded = await ConsentPurposeLoader.LoadAsync(db, purpose.Id, cancellationToken);
        return ConsentPrivacyMapper.ToDto(loaded);
    }
}

public sealed record DeleteConsentPurposeCommand(Guid Id) : IRequest;

public sealed class DeleteConsentPurposeCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteConsentPurposeCommand>
{
    public async Task Handle(DeleteConsentPurposeCommand request, CancellationToken cancellationToken)
    {
        var purpose = await ConsentPurposeLoader.LoadAsync(db, request.Id, cancellationToken);

        var inUse = await db.ConsentRecords.AnyAsync(c => c.ConsentPurposeId == purpose.Id, cancellationToken);
        if (inUse)
        {
            throw new ConflictException("Cannot remove a consent purpose that still has consent records linked to it.");
        }

        purpose.IsDeleted = true;
        purpose.DeletedAt = dateTimeProvider.UtcNow;
        purpose.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.purpose_deleted", nameof(ConsentPurpose), purpose.Id.ToString(), cancellationToken: cancellationToken);
    }
}
