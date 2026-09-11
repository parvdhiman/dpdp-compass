using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.CreateFramework;

/// <summary>
/// Super Administrator only — this is the shared, platform-wide legal
/// content library every tenant reads from. See
/// docs/COMPLIANCE_CONTENT_GOVERNANCE.md and the Module 2 precedent
/// (role-permission templates) this mirrors.
/// </summary>
public sealed record CreateFrameworkCommand(
    string Name,
    string Code,
    string Jurisdiction,
    string IssuingAuthority,
    string? Description) : IRequest<FrameworkDto>;

public sealed class CreateFrameworkCommandValidator : AbstractValidator<CreateFrameworkCommand>
{
    public CreateFrameworkCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Jurisdiction).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IssuingAuthority).NotEmpty().MaximumLength(300);
    }
}

public sealed class CreateFrameworkCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateFrameworkCommand, FrameworkDto>
{
    public async Task<FrameworkDto> Handle(CreateFrameworkCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance framework library.");
        }

        var codeTaken = await db.Frameworks.AnyAsync(f => f.Code == request.Code, cancellationToken);
        if (codeTaken)
        {
            throw new ConflictException("A framework with this code already exists.");
        }

        var framework = new Framework
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim(),
            Jurisdiction = request.Jurisdiction.Trim(),
            IssuingAuthority = request.IssuingAuthority.Trim(),
            Description = request.Description,
        };

        db.Frameworks.Add(framework);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.framework_created", nameof(Framework), framework.Id.ToString(), newValue: new { framework.Name, framework.Code }, cancellationToken: cancellationToken);

        return ComplianceMapper.ToDto(framework);
    }
}
