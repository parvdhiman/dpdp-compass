using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Domain.Modules.DataInventory;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.DataInventory.Commands;

internal static class DataFlowLoader
{
    public static async Task<DataFlow> LoadForDetailAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.DataFlows
            .Include(f => f.ProcessingActivity)
            .Include(f => f.DataCategory)
            .Include(f => f.FromItSystem)
            .Include(f => f.FromDataCollectionSource)
            .Include(f => f.ToItSystem)
            .Include(f => f.ToProcessor)
            .Include(f => f.ToRecipient)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(DataFlow), id);
}

public sealed record CreateDataFlowCommand(
    string Name, string? Description, Guid? ProcessingActivityId, Guid? DataCategoryId,
    Guid? FromItSystemId, Guid? FromDataCollectionSourceId, string FromDescription,
    Guid? ToItSystemId, Guid? ToProcessorId, Guid? ToRecipientId, string ToDescription,
    string? TransferMechanism, bool IsCrossBorder, string? CrossBorderCountry) : IRequest<DataFlowDto>;

public sealed class CreateDataFlowCommandValidator : AbstractValidator<CreateDataFlowCommand>
{
    public CreateDataFlowCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.FromDescription).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ToDescription).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TransferMechanism).MaximumLength(300);
        RuleFor(x => x.CrossBorderCountry).NotEmpty().When(x => x.IsCrossBorder).WithMessage("crossBorderCountry is required when isCrossBorder is true.");
    }
}

public sealed class CreateDataFlowCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateDataFlowCommand, DataFlowDto>
{
    public async Task<DataFlowDto> Handle(CreateDataFlowCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a data flow.");
        }

        await ValidateReferencesAsync(db, request.ProcessingActivityId, request.DataCategoryId, request.FromItSystemId,
            request.FromDataCollectionSourceId, request.ToItSystemId, request.ToProcessorId, request.ToRecipientId, cancellationToken);

        var flow = new DataFlow
        {
            OrganisationId = organisationId,
            Name = request.Name.Trim(),
            Description = request.Description,
            ProcessingActivityId = request.ProcessingActivityId,
            DataCategoryId = request.DataCategoryId,
            FromItSystemId = request.FromItSystemId,
            FromDataCollectionSourceId = request.FromDataCollectionSourceId,
            FromDescription = request.FromDescription.Trim(),
            ToItSystemId = request.ToItSystemId,
            ToProcessorId = request.ToProcessorId,
            ToRecipientId = request.ToRecipientId,
            ToDescription = request.ToDescription.Trim(),
            TransferMechanism = request.TransferMechanism,
            IsCrossBorder = request.IsCrossBorder,
            CrossBorderCountry = request.IsCrossBorder ? request.CrossBorderCountry : null,
        };

        db.DataFlows.Add(flow);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.flow_created", nameof(DataFlow), flow.Id.ToString(), newValue: new { flow.Name }, cancellationToken: cancellationToken);

        var loaded = await DataFlowLoader.LoadForDetailAsync(db, flow.Id, cancellationToken);
        return DataInventoryMapper.ToDto(loaded);
    }

    internal static async Task ValidateReferencesAsync(
        IAppDbContext db, Guid? processingActivityId, Guid? dataCategoryId, Guid? fromItSystemId,
        Guid? fromDataCollectionSourceId, Guid? toItSystemId, Guid? toProcessorId, Guid? toRecipientId, CancellationToken cancellationToken)
    {
        if (processingActivityId is { } activityId && !await db.ProcessingActivities.AnyAsync(a => a.Id == activityId, cancellationToken))
        {
            throw new NotFoundException(nameof(ProcessingActivity), activityId);
        }

        if (dataCategoryId is { } categoryId && !await db.DataCategories.AnyAsync(c => c.Id == categoryId, cancellationToken))
        {
            throw new NotFoundException(nameof(DataCategory), categoryId);
        }

        if (fromItSystemId is { } fromSystemId && !await db.ItSystems.AnyAsync(s => s.Id == fromSystemId, cancellationToken))
        {
            throw new NotFoundException(nameof(ItSystem), fromSystemId);
        }

        if (fromDataCollectionSourceId is { } fromSourceId && !await db.DataCollectionSources.AnyAsync(s => s.Id == fromSourceId, cancellationToken))
        {
            throw new NotFoundException(nameof(DataCollectionSource), fromSourceId);
        }

        if (toItSystemId is { } toSystemId && !await db.ItSystems.AnyAsync(s => s.Id == toSystemId, cancellationToken))
        {
            throw new NotFoundException(nameof(ItSystem), toSystemId);
        }

        if (toProcessorId is { } processorId && !await db.Processors.AnyAsync(p => p.Id == processorId, cancellationToken))
        {
            throw new NotFoundException(nameof(Processor), processorId);
        }

        if (toRecipientId is { } recipientId && !await db.Recipients.AnyAsync(r => r.Id == recipientId, cancellationToken))
        {
            throw new NotFoundException(nameof(Recipient), recipientId);
        }
    }
}

public sealed record UpdateDataFlowCommand(
    Guid Id, string Name, string? Description, Guid? ProcessingActivityId, Guid? DataCategoryId,
    Guid? FromItSystemId, Guid? FromDataCollectionSourceId, string FromDescription,
    Guid? ToItSystemId, Guid? ToProcessorId, Guid? ToRecipientId, string ToDescription,
    string? TransferMechanism, bool IsCrossBorder, string? CrossBorderCountry) : IRequest<DataFlowDto>;

public sealed class UpdateDataFlowCommandValidator : AbstractValidator<UpdateDataFlowCommand>
{
    public UpdateDataFlowCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.FromDescription).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ToDescription).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TransferMechanism).MaximumLength(300);
        RuleFor(x => x.CrossBorderCountry).NotEmpty().When(x => x.IsCrossBorder).WithMessage("crossBorderCountry is required when isCrossBorder is true.");
    }
}

public sealed class UpdateDataFlowCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<UpdateDataFlowCommand, DataFlowDto>
{
    public async Task<DataFlowDto> Handle(UpdateDataFlowCommand request, CancellationToken cancellationToken)
    {
        var flow = await DataFlowLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        await CreateDataFlowCommandHandler.ValidateReferencesAsync(db, request.ProcessingActivityId, request.DataCategoryId, request.FromItSystemId,
            request.FromDataCollectionSourceId, request.ToItSystemId, request.ToProcessorId, request.ToRecipientId, cancellationToken);

        flow.Name = request.Name.Trim();
        flow.Description = request.Description;
        flow.ProcessingActivityId = request.ProcessingActivityId;
        flow.DataCategoryId = request.DataCategoryId;
        flow.FromItSystemId = request.FromItSystemId;
        flow.FromDataCollectionSourceId = request.FromDataCollectionSourceId;
        flow.FromDescription = request.FromDescription.Trim();
        flow.ToItSystemId = request.ToItSystemId;
        flow.ToProcessorId = request.ToProcessorId;
        flow.ToRecipientId = request.ToRecipientId;
        flow.ToDescription = request.ToDescription.Trim();
        flow.TransferMechanism = request.TransferMechanism;
        flow.IsCrossBorder = request.IsCrossBorder;
        flow.CrossBorderCountry = request.IsCrossBorder ? request.CrossBorderCountry : null;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.flow_updated", nameof(DataFlow), flow.Id.ToString(), cancellationToken: cancellationToken);

        var loaded = await DataFlowLoader.LoadForDetailAsync(db, flow.Id, cancellationToken);
        return DataInventoryMapper.ToDto(loaded);
    }
}

public sealed record DeleteDataFlowCommand(Guid Id) : IRequest;

public sealed class DeleteDataFlowCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteDataFlowCommand>
{
    public async Task Handle(DeleteDataFlowCommand request, CancellationToken cancellationToken)
    {
        var flow = await DataFlowLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        flow.IsDeleted = true;
        flow.DeletedAt = dateTimeProvider.UtcNow;
        flow.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.flow_deleted", nameof(DataFlow), flow.Id.ToString(), cancellationToken: cancellationToken);
    }
}
