using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Domain.Modules.ConsentPrivacy;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.ConsentPrivacy.Commands;

internal static class SlaPolicyLoader
{
    public static async Task<SlaPolicy> LoadAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.SlaPolicies.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(SlaPolicy), id);
}

public sealed record CreateSlaPolicyCommand(string Name, string? Description, string? RequestType, int ResponseDueDays) : IRequest<SlaPolicyDto>;

public sealed class CreateSlaPolicyCommandValidator : AbstractValidator<CreateSlaPolicyCommand>
{
    public CreateSlaPolicyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ResponseDueDays).GreaterThan(0);
        RuleFor(x => x.RequestType)
            .Must(v => v is null || Enum.TryParse<DataPrincipalRequestType>(v, out _))
            .WithMessage("requestType must be one of: " + string.Join(", ", Enum.GetNames<DataPrincipalRequestType>()));
    }
}

public sealed class CreateSlaPolicyCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateSlaPolicyCommand, SlaPolicyDto>
{
    public async Task<SlaPolicyDto> Handle(CreateSlaPolicyCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create an SLA policy.");
        }

        var requestType = request.RequestType is null ? (DataPrincipalRequestType?)null : Enum.Parse<DataPrincipalRequestType>(request.RequestType);

        if (requestType is { } type && await db.SlaPolicies.AnyAsync(p => p.RequestType == type && p.IsActive, cancellationToken))
        {
            throw new ConflictException($"An active SLA policy already covers {type} requests. Deactivate it first, or edit it instead of creating another.");
        }

        var policy = new SlaPolicy
        {
            OrganisationId = organisationId,
            Name = request.Name.Trim(),
            Description = request.Description,
            RequestType = requestType,
            ResponseDueDays = request.ResponseDueDays,
        };

        db.SlaPolicies.Add(policy);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.slapolicy_created", nameof(SlaPolicy), policy.Id.ToString(), newValue: new { policy.Name, policy.ResponseDueDays }, cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDto(policy);
    }
}

public sealed record UpdateSlaPolicyCommand(Guid Id, string Name, string? Description, string? RequestType, int ResponseDueDays, bool IsActive) : IRequest<SlaPolicyDto>;

public sealed class UpdateSlaPolicyCommandValidator : AbstractValidator<UpdateSlaPolicyCommand>
{
    public UpdateSlaPolicyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ResponseDueDays).GreaterThan(0);
        RuleFor(x => x.RequestType)
            .Must(v => v is null || Enum.TryParse<DataPrincipalRequestType>(v, out _))
            .WithMessage("requestType must be one of: " + string.Join(", ", Enum.GetNames<DataPrincipalRequestType>()));
    }
}

public sealed class UpdateSlaPolicyCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<UpdateSlaPolicyCommand, SlaPolicyDto>
{
    public async Task<SlaPolicyDto> Handle(UpdateSlaPolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await SlaPolicyLoader.LoadAsync(db, request.Id, cancellationToken);
        var requestType = request.RequestType is null ? (DataPrincipalRequestType?)null : Enum.Parse<DataPrincipalRequestType>(request.RequestType);

        if (request.IsActive && requestType is { } type &&
            await db.SlaPolicies.AnyAsync(p => p.Id != policy.Id && p.RequestType == type && p.IsActive, cancellationToken))
        {
            throw new ConflictException($"An active SLA policy already covers {type} requests.");
        }

        policy.Name = request.Name.Trim();
        policy.Description = request.Description;
        policy.RequestType = requestType;
        policy.ResponseDueDays = request.ResponseDueDays;
        policy.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.slapolicy_updated", nameof(SlaPolicy), policy.Id.ToString(), cancellationToken: cancellationToken);

        return ConsentPrivacyMapper.ToDto(policy);
    }
}

public sealed record DeleteSlaPolicyCommand(Guid Id) : IRequest;

public sealed class DeleteSlaPolicyCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteSlaPolicyCommand>
{
    public async Task Handle(DeleteSlaPolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await SlaPolicyLoader.LoadAsync(db, request.Id, cancellationToken);

        policy.IsDeleted = true;
        policy.DeletedAt = dateTimeProvider.UtcNow;
        policy.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("consentprivacy.slapolicy_deleted", nameof(SlaPolicy), policy.Id.ToString(), cancellationToken: cancellationToken);
    }
}
