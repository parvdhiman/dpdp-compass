using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Domain.Modules.DataInventory;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.DataInventory.Commands;

internal static class ItSystemLoader
{
    public static async Task<ItSystem> LoadAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.ItSystems.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(ItSystem), id);
}

public sealed record CreateItSystemCommand(string Name, string? Description, string SystemType, Guid? OwnerUserId) : IRequest<ItSystemDto>;

public sealed class CreateItSystemCommandValidator : AbstractValidator<CreateItSystemCommand>
{
    public CreateItSystemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.SystemType).Must(v => Enum.TryParse<ItSystemType>(v, out _)).WithMessage("systemType must be one of: " + string.Join(", ", Enum.GetNames<ItSystemType>()));
    }
}

public sealed class CreateItSystemCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger) : IRequestHandler<CreateItSystemCommand, ItSystemDto>
{
    public async Task<ItSystemDto> Handle(CreateItSystemCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a system.");
        }

        var owner = request.OwnerUserId is { } ownerId
            ? await db.Users.FirstOrDefaultAsync(u => u.Id == ownerId, cancellationToken) ?? throw new NotFoundException(nameof(User), ownerId)
            : null;

        var system = new ItSystem
        {
            OrganisationId = organisationId,
            Name = request.Name.Trim(),
            Description = request.Description,
            SystemType = Enum.Parse<ItSystemType>(request.SystemType),
            OwnerUserId = owner?.Id,
            Owner = owner,
        };

        db.ItSystems.Add(system);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.system_created", nameof(ItSystem), system.Id.ToString(), newValue: new { system.Name }, cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDto(system);
    }
}

public sealed record UpdateItSystemCommand(Guid Id, string Name, string? Description, string SystemType, Guid? OwnerUserId, bool IsActive) : IRequest<ItSystemDto>;

public sealed class UpdateItSystemCommandValidator : AbstractValidator<UpdateItSystemCommand>
{
    public UpdateItSystemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.SystemType).Must(v => Enum.TryParse<ItSystemType>(v, out _)).WithMessage("systemType must be one of: " + string.Join(", ", Enum.GetNames<ItSystemType>()));
    }
}

public sealed class UpdateItSystemCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<UpdateItSystemCommand, ItSystemDto>
{
    public async Task<ItSystemDto> Handle(UpdateItSystemCommand request, CancellationToken cancellationToken)
    {
        var system = await ItSystemLoader.LoadAsync(db, request.Id, cancellationToken);

        var owner = request.OwnerUserId is { } ownerId
            ? await db.Users.FirstOrDefaultAsync(u => u.Id == ownerId, cancellationToken) ?? throw new NotFoundException(nameof(User), ownerId)
            : null;

        system.Name = request.Name.Trim();
        system.Description = request.Description;
        system.SystemType = Enum.Parse<ItSystemType>(request.SystemType);
        system.OwnerUserId = owner?.Id;
        system.Owner = owner;
        system.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.system_updated", nameof(ItSystem), system.Id.ToString(), cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDto(system);
    }
}

public sealed record DeleteItSystemCommand(Guid Id) : IRequest;

public sealed class DeleteItSystemCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteItSystemCommand>
{
    public async Task Handle(DeleteItSystemCommand request, CancellationToken cancellationToken)
    {
        var system = await ItSystemLoader.LoadAsync(db, request.Id, cancellationToken);

        var inUse = await db.DataInventoryItems.AnyAsync(i => i.ItSystemId == system.Id, cancellationToken);
        if (inUse)
        {
            throw new ConflictException("Cannot remove a system that is still referenced by a data inventory item.");
        }

        system.IsDeleted = true;
        system.DeletedAt = dateTimeProvider.UtcNow;
        system.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.system_deleted", nameof(ItSystem), system.Id.ToString(), cancellationToken: cancellationToken);
    }
}
