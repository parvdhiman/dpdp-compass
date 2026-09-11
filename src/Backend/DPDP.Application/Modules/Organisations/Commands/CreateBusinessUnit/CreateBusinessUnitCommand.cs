using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Domain.Common;
using DPDP.Domain.Modules.Organisations;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Commands.CreateBusinessUnit;

public sealed record CreateBusinessUnitCommand(
    Guid OrganisationId,
    string Name,
    string? Description,
    string? HeadName,
    string? HeadEmail,
    string? HeadPhone) : IRequest<BusinessUnitDto>;

public sealed class CreateBusinessUnitCommandValidator : AbstractValidator<CreateBusinessUnitCommand>
{
    public CreateBusinessUnitCommandValidator()
    {
        RuleFor(x => x.OrganisationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.HeadEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.HeadEmail));
    }
}

/// <summary>
/// Same tenant-boundary shape as CreateUserCommand in Module 2: a
/// non-Super-Administrator may only create within their own organisation.
/// </summary>
public sealed class CreateBusinessUnitCommandHandler(
    IAppDbContext db,
    ICurrentUserContext currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<CreateBusinessUnitCommand, BusinessUnitDto>
{
    public async Task<BusinessUnitDto> Handle(CreateBusinessUnitCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator && request.OrganisationId != currentUser.OrganisationId)
        {
            throw new ForbiddenException("Cannot create a business unit in another organisation.");
        }

        var organisationExists = await db.Organisations.AnyAsync(o => o.Id == request.OrganisationId, cancellationToken);
        if (!organisationExists)
        {
            throw new NotFoundException(nameof(Organisation), request.OrganisationId);
        }

        var nameTaken = await db.BusinessUnits
            .AnyAsync(b => b.OrganisationId == request.OrganisationId && b.Name == request.Name, cancellationToken);
        if (nameTaken)
        {
            throw new ConflictException("A business unit with this name already exists in this organisation.");
        }

        var businessUnit = new BusinessUnit
        {
            OrganisationId = request.OrganisationId,
            Name = request.Name.Trim(),
            Description = request.Description,
            Head = new ContactInfo { Name = request.HeadName, Email = request.HeadEmail, Phone = request.HeadPhone },
        };

        db.BusinessUnits.Add(businessUnit);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("business_unit.created", nameof(BusinessUnit), businessUnit.Id.ToString(), newValue: new { businessUnit.Name }, cancellationToken: cancellationToken);

        return new BusinessUnitDto(
            businessUnit.Id, businessUnit.OrganisationId, businessUnit.Name, businessUnit.Description,
            OrganisationMapper.ToDto(businessUnit.Head), businessUnit.IsActive, 0, businessUnit.CreatedAt);
    }
}
