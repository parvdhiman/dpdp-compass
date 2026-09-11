using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.UpdateFramework;

public sealed record UpdateFrameworkCommand(
    Guid Id,
    string Name,
    string Jurisdiction,
    string IssuingAuthority,
    string? Description) : IRequest<FrameworkDto>;

public sealed class UpdateFrameworkCommandValidator : AbstractValidator<UpdateFrameworkCommand>
{
    public UpdateFrameworkCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Jurisdiction).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IssuingAuthority).NotEmpty().MaximumLength(300);
    }
}

public sealed class UpdateFrameworkCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<UpdateFrameworkCommand, FrameworkDto>
{
    public async Task<FrameworkDto> Handle(UpdateFrameworkCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance framework library.");
        }

        var framework = await db.Frameworks
            .Include(f => f.Versions)
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Framework), request.Id);

        framework.Name = request.Name.Trim();
        framework.Jurisdiction = request.Jurisdiction.Trim();
        framework.IssuingAuthority = request.IssuingAuthority.Trim();
        framework.Description = request.Description;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.framework_updated", nameof(Framework), framework.Id.ToString(), cancellationToken: cancellationToken);

        return ComplianceMapper.ToDto(framework);
    }
}
