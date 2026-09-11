using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Domain.Common;
using DPDP.Domain.Modules.Organisations;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Commands.UpdateBusinessUnit;

public sealed record UpdateBusinessUnitCommand(
    Guid Id,
    string Name,
    string? Description,
    string? HeadName,
    string? HeadEmail,
    string? HeadPhone,
    bool IsActive) : IRequest<BusinessUnitDto>;

public sealed class UpdateBusinessUnitCommandValidator : AbstractValidator<UpdateBusinessUnitCommand>
{
    public UpdateBusinessUnitCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.HeadEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.HeadEmail));
    }
}

/// <summary>The query filter already prevents updating another tenant's business unit.</summary>
public sealed class UpdateBusinessUnitCommandHandler(
    IAppDbContext db,
    IAuditLogger auditLogger)
    : IRequestHandler<UpdateBusinessUnitCommand, BusinessUnitDto>
{
    public async Task<BusinessUnitDto> Handle(UpdateBusinessUnitCommand request, CancellationToken cancellationToken)
    {
        var businessUnit = await db.BusinessUnits
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(BusinessUnit), request.Id);

        var nameTaken = await db.BusinessUnits
            .AnyAsync(b => b.OrganisationId == businessUnit.OrganisationId && b.Name == request.Name && b.Id != request.Id, cancellationToken);
        if (nameTaken)
        {
            throw new ConflictException("A business unit with this name already exists in this organisation.");
        }

        var oldValue = new { businessUnit.Name, businessUnit.IsActive };

        businessUnit.Name = request.Name.Trim();
        businessUnit.Description = request.Description;
        businessUnit.Head = new ContactInfo { Name = request.HeadName, Email = request.HeadEmail, Phone = request.HeadPhone };
        businessUnit.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("business_unit.updated", nameof(BusinessUnit), businessUnit.Id.ToString(), oldValue, new { businessUnit.Name, businessUnit.IsActive }, cancellationToken);

        var departmentCount = await db.Departments.CountAsync(d => d.BusinessUnitId == businessUnit.Id, cancellationToken);

        return new BusinessUnitDto(
            businessUnit.Id, businessUnit.OrganisationId, businessUnit.Name, businessUnit.Description,
            OrganisationMapper.ToDto(businessUnit.Head), businessUnit.IsActive, departmentCount, businessUnit.CreatedAt);
    }
}
