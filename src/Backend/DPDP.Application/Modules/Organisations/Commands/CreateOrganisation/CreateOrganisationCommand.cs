using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Domain.Common;
using DPDP.Domain.Modules.Organisations;
using FluentValidation;
using MediatR;

namespace DPDP.Application.Modules.Organisations.Commands.CreateOrganisation;

/// <summary>
/// Provisions a brand-new tenant. Distinct from the org-less Super
/// Administrator bootstrap in Module 2 (IdentityBootstrapper) — this
/// creates an *organisation* only, no user. Super Administrator only:
/// provisioning a tenant is a platform-level action, not something any
/// Organisation Administrator (who by definition already belongs to one
/// organisation) should be able to do.
/// </summary>
public sealed record CreateOrganisationCommand(
    string Name,
    string? LegalName,
    string? Industry,
    string? Size,
    string? Country,
    string? Website,
    string? PrimaryContactName,
    string? PrimaryContactEmail,
    string? PrimaryContactPhone) : IRequest<OrganisationProfileDto>;

public sealed class CreateOrganisationCommandValidator : AbstractValidator<CreateOrganisationCommand>
{
    public CreateOrganisationCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LegalName).MaximumLength(300);
        RuleFor(x => x.Industry).MaximumLength(100);
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.Website).MaximumLength(300);
        RuleFor(x => x.Size)
            .Must(size => size is null || Enum.TryParse<OrganisationSize>(size, out _))
            .WithMessage("Size must be one of: " + string.Join(", ", Enum.GetNames<OrganisationSize>()));
        RuleFor(x => x.PrimaryContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.PrimaryContactEmail));
    }
}

public sealed class CreateOrganisationCommandHandler(
    IAppDbContext db,
    ICurrentUserContext currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<CreateOrganisationCommand, OrganisationProfileDto>
{
    public async Task<OrganisationProfileDto> Handle(CreateOrganisationCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can provision a new organisation.");
        }

        var organisation = new Organisation
        {
            Name = request.Name.Trim(),
            LegalName = request.LegalName,
            Status = OrganisationStatus.Active,
            Industry = request.Industry,
            Size = request.Size is null ? null : Enum.Parse<OrganisationSize>(request.Size),
            Country = request.Country,
            Website = request.Website,
            PrimaryContact = new ContactInfo
            {
                Name = request.PrimaryContactName,
                Email = request.PrimaryContactEmail,
                Phone = request.PrimaryContactPhone,
            },
        };

        db.Organisations.Add(organisation);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("organisation.created", nameof(Organisation), organisation.Id.ToString(), newValue: new { organisation.Name }, cancellationToken: cancellationToken);

        return OrganisationMapper.ToProfileDto(organisation);
    }
}
