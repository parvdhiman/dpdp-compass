using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Domain.Modules.DataInventory;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.DataInventory.Commands;

internal static class DataCollectionSourceLoader
{
    public static async Task<DataCollectionSource> LoadAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.DataCollectionSources.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(DataCollectionSource), id);
}

public sealed record CreateDataCollectionSourceCommand(string Name, string? Description, string SourceType) : IRequest<DataCollectionSourceDto>;

public sealed class CreateDataCollectionSourceCommandValidator : AbstractValidator<CreateDataCollectionSourceCommand>
{
    public CreateDataCollectionSourceCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.SourceType).Must(v => Enum.TryParse<DataCollectionSourceType>(v, out _)).WithMessage("sourceType must be one of: " + string.Join(", ", Enum.GetNames<DataCollectionSourceType>()));
    }
}

public sealed class CreateDataCollectionSourceCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateDataCollectionSourceCommand, DataCollectionSourceDto>
{
    public async Task<DataCollectionSourceDto> Handle(CreateDataCollectionSourceCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a data collection source.");
        }

        var source = new DataCollectionSource
        {
            OrganisationId = organisationId,
            Name = request.Name.Trim(),
            Description = request.Description,
            SourceType = Enum.Parse<DataCollectionSourceType>(request.SourceType),
        };

        db.DataCollectionSources.Add(source);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.collectionsource_created", nameof(DataCollectionSource), source.Id.ToString(), newValue: new { source.Name }, cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDto(source);
    }
}

public sealed record UpdateDataCollectionSourceCommand(Guid Id, string Name, string? Description, string SourceType, bool IsActive) : IRequest<DataCollectionSourceDto>;

public sealed class UpdateDataCollectionSourceCommandValidator : AbstractValidator<UpdateDataCollectionSourceCommand>
{
    public UpdateDataCollectionSourceCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.SourceType).Must(v => Enum.TryParse<DataCollectionSourceType>(v, out _)).WithMessage("sourceType must be one of: " + string.Join(", ", Enum.GetNames<DataCollectionSourceType>()));
    }
}

public sealed class UpdateDataCollectionSourceCommandHandler(IAppDbContext db, IAuditLogger auditLogger)
    : IRequestHandler<UpdateDataCollectionSourceCommand, DataCollectionSourceDto>
{
    public async Task<DataCollectionSourceDto> Handle(UpdateDataCollectionSourceCommand request, CancellationToken cancellationToken)
    {
        var source = await DataCollectionSourceLoader.LoadAsync(db, request.Id, cancellationToken);

        source.Name = request.Name.Trim();
        source.Description = request.Description;
        source.SourceType = Enum.Parse<DataCollectionSourceType>(request.SourceType);
        source.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.collectionsource_updated", nameof(DataCollectionSource), source.Id.ToString(), cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDto(source);
    }
}

public sealed record DeleteDataCollectionSourceCommand(Guid Id) : IRequest;

public sealed class DeleteDataCollectionSourceCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteDataCollectionSourceCommand>
{
    public async Task Handle(DeleteDataCollectionSourceCommand request, CancellationToken cancellationToken)
    {
        var source = await DataCollectionSourceLoader.LoadAsync(db, request.Id, cancellationToken);

        var inUse = await db.DataInventoryItems.AnyAsync(i => i.DataCollectionSourceId == source.Id, cancellationToken);
        if (inUse)
        {
            throw new ConflictException("Cannot remove a data collection source that is still referenced by a data inventory item.");
        }

        source.IsDeleted = true;
        source.DeletedAt = dateTimeProvider.UtcNow;
        source.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.collectionsource_deleted", nameof(DataCollectionSource), source.Id.ToString(), cancellationToken: cancellationToken);
    }
}
