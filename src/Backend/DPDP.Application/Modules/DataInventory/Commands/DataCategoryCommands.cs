using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Domain.Modules.DataDiscovery;
using DPDP.Domain.Modules.DataInventory;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.DataInventory.Commands;

internal static class DataCategoryLoader
{
    public static async Task<DataCategory> LoadAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.DataCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(DataCategory), id);
}

public sealed record CreateDataCategoryCommand(string Name, string? Description, string? ClassificationCategory) : IRequest<DataCategoryDto>;

public sealed class CreateDataCategoryCommandValidator : AbstractValidator<CreateDataCategoryCommand>
{
    public CreateDataCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ClassificationCategory)
            .Must(v => v is null || Enum.TryParse<ClassificationCategory>(v, out _))
            .WithMessage("classificationCategory must be one of: " + string.Join(", ", Enum.GetNames<ClassificationCategory>()));
    }
}

public sealed class CreateDataCategoryCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateDataCategoryCommand, DataCategoryDto>
{
    public async Task<DataCategoryDto> Handle(CreateDataCategoryCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a data category.");
        }

        var category = new DataCategory
        {
            OrganisationId = organisationId,
            Name = request.Name.Trim(),
            Description = request.Description,
            ClassificationCategory = request.ClassificationCategory is null ? null : Enum.Parse<ClassificationCategory>(request.ClassificationCategory),
        };

        db.DataCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.category_created", nameof(DataCategory), category.Id.ToString(), newValue: new { category.Name }, cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDto(category);
    }
}

public sealed record UpdateDataCategoryCommand(Guid Id, string Name, string? Description, string? ClassificationCategory, bool IsActive) : IRequest<DataCategoryDto>;

public sealed class UpdateDataCategoryCommandValidator : AbstractValidator<UpdateDataCategoryCommand>
{
    public UpdateDataCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ClassificationCategory)
            .Must(v => v is null || Enum.TryParse<ClassificationCategory>(v, out _))
            .WithMessage("classificationCategory must be one of: " + string.Join(", ", Enum.GetNames<ClassificationCategory>()));
    }
}

public sealed class UpdateDataCategoryCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<UpdateDataCategoryCommand, DataCategoryDto>
{
    public async Task<DataCategoryDto> Handle(UpdateDataCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await DataCategoryLoader.LoadAsync(db, request.Id, cancellationToken);

        category.Name = request.Name.Trim();
        category.Description = request.Description;
        category.ClassificationCategory = request.ClassificationCategory is null ? null : Enum.Parse<ClassificationCategory>(request.ClassificationCategory);
        category.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.category_updated", nameof(DataCategory), category.Id.ToString(), cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDto(category);
    }
}

public sealed record DeleteDataCategoryCommand(Guid Id) : IRequest;

public sealed class DeleteDataCategoryCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteDataCategoryCommand>
{
    public async Task Handle(DeleteDataCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await DataCategoryLoader.LoadAsync(db, request.Id, cancellationToken);

        var inUse = await db.DataInventoryItems.AnyAsync(i => i.DataCategoryId == category.Id, cancellationToken);
        if (inUse)
        {
            throw new ConflictException("Cannot remove a data category that is still referenced by a data inventory item.");
        }

        category.IsDeleted = true;
        category.DeletedAt = dateTimeProvider.UtcNow;
        category.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.category_deleted", nameof(DataCategory), category.Id.ToString(), cancellationToken: cancellationToken);
    }
}
