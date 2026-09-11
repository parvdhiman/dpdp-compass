using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Domain.Modules.DataInventory;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.DataInventory.Commands;

internal static class ProcessorLoader
{
    public static async Task<Processor> LoadAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.Processors.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Processor), id);
}

public sealed record CreateProcessorCommand(string Name, string? Description, string? ContactEmail, string? Country) : IRequest<ProcessorDto>;

public sealed class CreateProcessorCommandValidator : AbstractValidator<CreateProcessorCommand>
{
    public CreateProcessorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ContactEmail).EmailAddress().MaximumLength(300).When(x => !string.IsNullOrEmpty(x.ContactEmail));
        RuleFor(x => x.Country).MaximumLength(100);
    }
}

public sealed class CreateProcessorCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger) : IRequestHandler<CreateProcessorCommand, ProcessorDto>
{
    public async Task<ProcessorDto> Handle(CreateProcessorCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a processor.");
        }

        var processor = new Processor
        {
            OrganisationId = organisationId,
            Name = request.Name.Trim(),
            Description = request.Description,
            ContactEmail = request.ContactEmail,
            Country = request.Country,
        };

        db.Processors.Add(processor);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.processor_created", nameof(Processor), processor.Id.ToString(), newValue: new { processor.Name }, cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDto(processor);
    }
}

public sealed record UpdateProcessorCommand(Guid Id, string Name, string? Description, string? ContactEmail, string? Country, bool IsActive) : IRequest<ProcessorDto>;

public sealed class UpdateProcessorCommandValidator : AbstractValidator<UpdateProcessorCommand>
{
    public UpdateProcessorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ContactEmail).EmailAddress().MaximumLength(300).When(x => !string.IsNullOrEmpty(x.ContactEmail));
        RuleFor(x => x.Country).MaximumLength(100);
    }
}

public sealed class UpdateProcessorCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<UpdateProcessorCommand, ProcessorDto>
{
    public async Task<ProcessorDto> Handle(UpdateProcessorCommand request, CancellationToken cancellationToken)
    {
        var processor = await ProcessorLoader.LoadAsync(db, request.Id, cancellationToken);

        processor.Name = request.Name.Trim();
        processor.Description = request.Description;
        processor.ContactEmail = request.ContactEmail;
        processor.Country = request.Country;
        processor.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.processor_updated", nameof(Processor), processor.Id.ToString(), cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDto(processor);
    }
}

public sealed record DeleteProcessorCommand(Guid Id) : IRequest;

public sealed class DeleteProcessorCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteProcessorCommand>
{
    public async Task Handle(DeleteProcessorCommand request, CancellationToken cancellationToken)
    {
        var processor = await ProcessorLoader.LoadAsync(db, request.Id, cancellationToken);

        var inUseByInventory = await db.DataInventoryItems.AnyAsync(i => i.ProcessorId == processor.Id, cancellationToken);
        var inUseByActivity = await db.ProcessingActivities.AnyAsync(a => a.Processors.Any(p => p.Id == processor.Id), cancellationToken);
        if (inUseByInventory || inUseByActivity)
        {
            throw new ConflictException("Cannot remove a processor that is still referenced by a data inventory item or processing activity.");
        }

        processor.IsDeleted = true;
        processor.DeletedAt = dateTimeProvider.UtcNow;
        processor.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.processor_deleted", nameof(Processor), processor.Id.ToString(), cancellationToken: cancellationToken);
    }
}
