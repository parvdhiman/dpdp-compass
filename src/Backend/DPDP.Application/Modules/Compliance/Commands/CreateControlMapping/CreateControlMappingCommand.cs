using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.CreateControlMapping;

public sealed record CreateControlMappingCommand(Guid ControlId, Guid RequirementId, string? MappingNotes) : IRequest<Guid>;

public sealed class CreateControlMappingCommandValidator : AbstractValidator<CreateControlMappingCommand>
{
    public CreateControlMappingCommandValidator()
    {
        RuleFor(x => x.ControlId).NotEmpty();
        RuleFor(x => x.RequirementId).NotEmpty();
    }
}

public sealed class CreateControlMappingCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateControlMappingCommand, Guid>
{
    public async Task<Guid> Handle(CreateControlMappingCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance control library.");
        }

        var controlExists = await db.Controls.AnyAsync(c => c.Id == request.ControlId, cancellationToken);
        if (!controlExists)
        {
            throw new NotFoundException(nameof(Control), request.ControlId);
        }

        var requirementExists = await db.Requirements.AnyAsync(r => r.Id == request.RequirementId, cancellationToken);
        if (!requirementExists)
        {
            throw new NotFoundException(nameof(Requirement), request.RequirementId);
        }

        var alreadyMapped = await db.ControlMappings
            .AnyAsync(m => m.ControlId == request.ControlId && m.RequirementId == request.RequirementId, cancellationToken);
        if (alreadyMapped)
        {
            throw new ConflictException("This control is already mapped to this requirement.");
        }

        var mapping = new ControlMapping
        {
            ControlId = request.ControlId,
            RequirementId = request.RequirementId,
            MappingNotes = request.MappingNotes,
        };

        db.ControlMappings.Add(mapping);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.control_mapping_created", nameof(ControlMapping), mapping.Id.ToString(), cancellationToken: cancellationToken);

        return mapping.Id;
    }
}
