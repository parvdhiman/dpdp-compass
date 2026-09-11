using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataInventory.DTOs;
using DPDP.Domain.Modules.DataInventory;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.DataInventory.Commands;

internal static class RetentionPolicyLoader
{
    public static async Task<RetentionPolicy> LoadAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.RetentionPolicies.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(RetentionPolicy), id);
}

public sealed record CreateRetentionPolicyCommand(string Name, string? Description, int RetentionPeriodValue, string RetentionPeriodUnit, string? TriggerEvent) : IRequest<RetentionPolicyDto>;

public sealed class CreateRetentionPolicyCommandValidator : AbstractValidator<CreateRetentionPolicyCommand>
{
    public CreateRetentionPolicyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.RetentionPeriodValue).GreaterThan(0);
        RuleFor(x => x.RetentionPeriodUnit).Must(v => Enum.TryParse<RetentionPeriodUnit>(v, out _)).WithMessage("retentionPeriodUnit must be one of: " + string.Join(", ", Enum.GetNames<RetentionPeriodUnit>()));
        RuleFor(x => x.TriggerEvent).MaximumLength(500);
    }
}

public sealed class CreateRetentionPolicyCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateRetentionPolicyCommand, RetentionPolicyDto>
{
    public async Task<RetentionPolicyDto> Handle(CreateRetentionPolicyCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a retention policy.");
        }

        var policy = new RetentionPolicy
        {
            OrganisationId = organisationId,
            Name = request.Name.Trim(),
            Description = request.Description,
            RetentionPeriodValue = request.RetentionPeriodValue,
            RetentionPeriodUnit = Enum.Parse<RetentionPeriodUnit>(request.RetentionPeriodUnit),
            TriggerEvent = request.TriggerEvent,
        };

        db.RetentionPolicies.Add(policy);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.retentionpolicy_created", nameof(RetentionPolicy), policy.Id.ToString(), newValue: new { policy.Name }, cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDto(policy);
    }
}

public sealed record UpdateRetentionPolicyCommand(Guid Id, string Name, string? Description, int RetentionPeriodValue, string RetentionPeriodUnit, string? TriggerEvent, bool IsActive) : IRequest<RetentionPolicyDto>;

public sealed class UpdateRetentionPolicyCommandValidator : AbstractValidator<UpdateRetentionPolicyCommand>
{
    public UpdateRetentionPolicyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.RetentionPeriodValue).GreaterThan(0);
        RuleFor(x => x.RetentionPeriodUnit).Must(v => Enum.TryParse<RetentionPeriodUnit>(v, out _)).WithMessage("retentionPeriodUnit must be one of: " + string.Join(", ", Enum.GetNames<RetentionPeriodUnit>()));
        RuleFor(x => x.TriggerEvent).MaximumLength(500);
    }
}

public sealed class UpdateRetentionPolicyCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<UpdateRetentionPolicyCommand, RetentionPolicyDto>
{
    public async Task<RetentionPolicyDto> Handle(UpdateRetentionPolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await RetentionPolicyLoader.LoadAsync(db, request.Id, cancellationToken);

        policy.Name = request.Name.Trim();
        policy.Description = request.Description;
        policy.RetentionPeriodValue = request.RetentionPeriodValue;
        policy.RetentionPeriodUnit = Enum.Parse<RetentionPeriodUnit>(request.RetentionPeriodUnit);
        policy.TriggerEvent = request.TriggerEvent;
        policy.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.retentionpolicy_updated", nameof(RetentionPolicy), policy.Id.ToString(), cancellationToken: cancellationToken);

        return DataInventoryMapper.ToDto(policy);
    }
}

public sealed record DeleteRetentionPolicyCommand(Guid Id) : IRequest;

public sealed class DeleteRetentionPolicyCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteRetentionPolicyCommand>
{
    public async Task Handle(DeleteRetentionPolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await RetentionPolicyLoader.LoadAsync(db, request.Id, cancellationToken);

        var inUseByInventory = await db.DataInventoryItems.AnyAsync(i => i.RetentionPolicyId == policy.Id, cancellationToken);
        var inUseByActivity = await db.ProcessingActivities.AnyAsync(a => a.RetentionPolicyId == policy.Id, cancellationToken);
        if (inUseByInventory || inUseByActivity)
        {
            throw new ConflictException("Cannot remove a retention policy that is still referenced by a data inventory item or processing activity.");
        }

        policy.IsDeleted = true;
        policy.DeletedAt = dateTimeProvider.UtcNow;
        policy.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datainventory.retentionpolicy_deleted", nameof(RetentionPolicy), policy.Id.ToString(), cancellationToken: cancellationToken);
    }
}
