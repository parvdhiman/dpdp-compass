using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Domain.Modules.DataInventory;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.DataInventory.Commands;

internal static class RecipientLoader
{
    public static async Task<Recipient> LoadAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.Recipients.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Recipient), id);
}

public sealed record CreateRecipientCommand(string Name, string? Description, string RecipientType) : IRequest<RecipientDto>;

public sealed class CreateRecipientCommandValidator : AbstractValidator<CreateRecipientCommand>
{
    public CreateRecipientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.RecipientType).Must(v => Enum.TryParse<Domain.Modules.DataInventory.RecipientType>(v, out _)).WithMessage("recipientType must be one of: " + string.Join(", ", Enum.GetNames<Domain.Modules.DataInventory.RecipientType>()));
    }
}

public sealed class CreateRecipientCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger) : IRequestHandler<CreateRecipientCommand, RecipientDto>
{
    public async Task<RecipientDto> Handle(CreateRecipientCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a recipient.");
        }

        var recipient = new Recipient
        {
            OrganisationId = organisationId,
            Name = request.Name.Trim(),
            Description = request.Description,
            RecipientType = Enum.Parse<Domain.Modules.DataInventory.RecipientType>(request.RecipientType),
        };

        db.Recipients.Add(recipient);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.recipient_created", nameof(Recipient), recipient.Id.ToString(), newValue: new { recipient.Name }, cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDto(recipient);
    }
}

public sealed record UpdateRecipientCommand(Guid Id, string Name, string? Description, string RecipientType, bool IsActive) : IRequest<RecipientDto>;

public sealed class UpdateRecipientCommandValidator : AbstractValidator<UpdateRecipientCommand>
{
    public UpdateRecipientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.RecipientType).Must(v => Enum.TryParse<Domain.Modules.DataInventory.RecipientType>(v, out _)).WithMessage("recipientType must be one of: " + string.Join(", ", Enum.GetNames<Domain.Modules.DataInventory.RecipientType>()));
    }
}

public sealed class UpdateRecipientCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<UpdateRecipientCommand, RecipientDto>
{
    public async Task<RecipientDto> Handle(UpdateRecipientCommand request, CancellationToken cancellationToken)
    {
        var recipient = await RecipientLoader.LoadAsync(db, request.Id, cancellationToken);

        recipient.Name = request.Name.Trim();
        recipient.Description = request.Description;
        recipient.RecipientType = Enum.Parse<Domain.Modules.DataInventory.RecipientType>(request.RecipientType);
        recipient.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.recipient_updated", nameof(Recipient), recipient.Id.ToString(), cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDto(recipient);
    }
}

public sealed record DeleteRecipientCommand(Guid Id) : IRequest;

public sealed class DeleteRecipientCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteRecipientCommand>
{
    public async Task Handle(DeleteRecipientCommand request, CancellationToken cancellationToken)
    {
        var recipient = await RecipientLoader.LoadAsync(db, request.Id, cancellationToken);

        var inUse = await db.ProcessingActivities.AnyAsync(a => a.Recipients.Any(r => r.Id == recipient.Id), cancellationToken);
        if (inUse)
        {
            throw new ConflictException("Cannot remove a recipient that is still referenced by a processing activity.");
        }

        recipient.IsDeleted = true;
        recipient.DeletedAt = dateTimeProvider.UtcNow;
        recipient.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.recipient_deleted", nameof(Recipient), recipient.Id.ToString(), cancellationToken: cancellationToken);
    }
}
