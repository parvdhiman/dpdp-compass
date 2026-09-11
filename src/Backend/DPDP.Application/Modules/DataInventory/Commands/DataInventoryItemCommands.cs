using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.DataDiscovery;
using DPDP.Domain.Modules.DataInventory;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.DataInventory.Commands;

internal static class DataInventoryItemLoader
{
    public static async Task<DataInventoryItem> LoadForDetailAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.DataInventoryItems
            .Include(i => i.DataCategory)
            .Include(i => i.DataCollectionSource)
            .Include(i => i.ItSystem)
            .Include(i => i.Owner)
            .Include(i => i.RetentionPolicy)
            .Include(i => i.Processor)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(DataInventoryItem), id);
}

public sealed record CreateDataInventoryItemCommand(
    Guid DataCategoryId, string DataElementName, Guid? DiscoveredDataElementId, string? Classification,
    Guid? DataCollectionSourceId, Guid? ItSystemId, Guid? OwnerUserId, string? Purpose,
    Guid? RetentionPolicyId, string? SharingDescription, Guid? ProcessorId, string? RiskLevel)
    : IRequest<DataInventoryItemDto>;

public sealed class CreateDataInventoryItemCommandValidator : AbstractValidator<CreateDataInventoryItemCommand>
{
    public CreateDataInventoryItemCommandValidator()
    {
        RuleFor(x => x.DataCategoryId).NotEmpty();
        RuleFor(x => x.DataElementName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Purpose).MaximumLength(2000);
        RuleFor(x => x.SharingDescription).MaximumLength(2000);
        RuleFor(x => x.Classification)
            .Must(v => v is null || Enum.TryParse<ClassificationCategory>(v, out _))
            .WithMessage("classification must be one of: " + string.Join(", ", Enum.GetNames<ClassificationCategory>()));
        RuleFor(x => x.RiskLevel)
            .Must(v => v is null || Enum.TryParse<RiskLevel>(v, out _))
            .WithMessage("riskLevel must be one of: " + string.Join(", ", Enum.GetNames<RiskLevel>()));
    }
}

public sealed class CreateDataInventoryItemCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateDataInventoryItemCommand, DataInventoryItemDto>
{
    public async Task<DataInventoryItemDto> Handle(CreateDataInventoryItemCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a data inventory item.");
        }

        if (!await db.DataCategories.AnyAsync(c => c.Id == request.DataCategoryId, cancellationToken))
        {
            throw new NotFoundException(nameof(DataCategory), request.DataCategoryId);
        }

        await ValidateReferencesAsync(db, request.DataCollectionSourceId, request.ItSystemId, request.OwnerUserId, request.RetentionPolicyId, request.ProcessorId, cancellationToken);

        var item = new DataInventoryItem
        {
            OrganisationId = organisationId,
            DataCategoryId = request.DataCategoryId,
            DataElementName = request.DataElementName.Trim(),
            DiscoveredDataElementId = request.DiscoveredDataElementId,
            Classification = request.Classification is null ? null : Enum.Parse<ClassificationCategory>(request.Classification),
            DataCollectionSourceId = request.DataCollectionSourceId,
            ItSystemId = request.ItSystemId,
            OwnerUserId = request.OwnerUserId,
            Purpose = request.Purpose,
            RetentionPolicyId = request.RetentionPolicyId,
            SharingDescription = request.SharingDescription,
            ProcessorId = request.ProcessorId,
            RiskLevel = request.RiskLevel is null ? null : Enum.Parse<RiskLevel>(request.RiskLevel),
        };

        db.DataInventoryItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.item_created", nameof(DataInventoryItem), item.Id.ToString(), newValue: new { item.DataElementName }, cancellationToken: cancellationToken);

        var loaded = await DataInventoryItemLoader.LoadForDetailAsync(db, item.Id, cancellationToken);
        return DataInventoryMapper.ToDto(loaded);
    }

    internal static async Task ValidateReferencesAsync(
        IAppDbContext db, Guid? dataCollectionSourceId, Guid? itSystemId, Guid? ownerUserId, Guid? retentionPolicyId, Guid? processorId, CancellationToken cancellationToken)
    {
        if (dataCollectionSourceId is { } sourceId && !await db.DataCollectionSources.AnyAsync(s => s.Id == sourceId, cancellationToken))
        {
            throw new NotFoundException(nameof(DataCollectionSource), sourceId);
        }

        if (itSystemId is { } systemId && !await db.ItSystems.AnyAsync(s => s.Id == systemId, cancellationToken))
        {
            throw new NotFoundException(nameof(ItSystem), systemId);
        }

        if (ownerUserId is { } userId && !await db.Users.AnyAsync(u => u.Id == userId, cancellationToken))
        {
            throw new NotFoundException(nameof(User), userId);
        }

        if (retentionPolicyId is { } policyId && !await db.RetentionPolicies.AnyAsync(p => p.Id == policyId, cancellationToken))
        {
            throw new NotFoundException(nameof(RetentionPolicy), policyId);
        }

        if (processorId is { } procId && !await db.Processors.AnyAsync(p => p.Id == procId, cancellationToken))
        {
            throw new NotFoundException(nameof(Processor), procId);
        }
    }
}

public sealed record UpdateDataInventoryItemCommand(
    Guid Id, Guid DataCategoryId, string DataElementName, string? Classification,
    Guid? DataCollectionSourceId, Guid? ItSystemId, Guid? OwnerUserId, string? Purpose,
    Guid? RetentionPolicyId, string? SharingDescription, Guid? ProcessorId, string? RiskLevel)
    : IRequest<DataInventoryItemDto>;

public sealed class UpdateDataInventoryItemCommandValidator : AbstractValidator<UpdateDataInventoryItemCommand>
{
    public UpdateDataInventoryItemCommandValidator()
    {
        RuleFor(x => x.DataCategoryId).NotEmpty();
        RuleFor(x => x.DataElementName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Purpose).MaximumLength(2000);
        RuleFor(x => x.SharingDescription).MaximumLength(2000);
        RuleFor(x => x.Classification)
            .Must(v => v is null || Enum.TryParse<ClassificationCategory>(v, out _))
            .WithMessage("classification must be one of: " + string.Join(", ", Enum.GetNames<ClassificationCategory>()));
        RuleFor(x => x.RiskLevel)
            .Must(v => v is null || Enum.TryParse<RiskLevel>(v, out _))
            .WithMessage("riskLevel must be one of: " + string.Join(", ", Enum.GetNames<RiskLevel>()));
    }
}

public sealed class UpdateDataInventoryItemCommandHandler(IAppDbContext db, IAuditLogger auditLogger)
    : IRequestHandler<UpdateDataInventoryItemCommand, DataInventoryItemDto>
{
    public async Task<DataInventoryItemDto> Handle(UpdateDataInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await DataInventoryItemLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!await db.DataCategories.AnyAsync(c => c.Id == request.DataCategoryId, cancellationToken))
        {
            throw new NotFoundException(nameof(DataCategory), request.DataCategoryId);
        }

        await CreateDataInventoryItemCommandHandler.ValidateReferencesAsync(
            db, request.DataCollectionSourceId, request.ItSystemId, request.OwnerUserId, request.RetentionPolicyId, request.ProcessorId, cancellationToken);

        item.DataCategoryId = request.DataCategoryId;
        item.DataElementName = request.DataElementName.Trim();
        item.Classification = request.Classification is null ? null : Enum.Parse<ClassificationCategory>(request.Classification);
        item.DataCollectionSourceId = request.DataCollectionSourceId;
        item.ItSystemId = request.ItSystemId;
        item.OwnerUserId = request.OwnerUserId;
        item.Purpose = request.Purpose;
        item.RetentionPolicyId = request.RetentionPolicyId;
        item.SharingDescription = request.SharingDescription;
        item.ProcessorId = request.ProcessorId;
        item.RiskLevel = request.RiskLevel is null ? null : Enum.Parse<RiskLevel>(request.RiskLevel);

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.item_updated", nameof(DataInventoryItem), item.Id.ToString(), cancellationToken: cancellationToken);

        var loaded = await DataInventoryItemLoader.LoadForDetailAsync(db, item.Id, cancellationToken);
        return DataInventoryMapper.ToDto(loaded);
    }
}

public sealed record DeleteDataInventoryItemCommand(Guid Id) : IRequest;

public sealed class DeleteDataInventoryItemCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteDataInventoryItemCommand>
{
    public async Task Handle(DeleteDataInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await DataInventoryItemLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        item.IsDeleted = true;
        item.DeletedAt = dateTimeProvider.UtcNow;
        item.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.item_deleted", nameof(DataInventoryItem), item.Id.ToString(), cancellationToken: cancellationToken);
    }
}
