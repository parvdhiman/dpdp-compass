using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Domain.Common;
using DPDP.Domain.Modules.Organisations;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Commands.UpdateOrganisationProfile;

public sealed record UpdateOrganisationProfileCommand(
    Guid OrganisationId,
    string Name,
    string? LegalName,
    string? Industry,
    string? Size,
    string? Country,
    string? Website,
    string? PrimaryContactName,
    string? PrimaryContactEmail,
    string? PrimaryContactPhone,
    string? PrivacyContactName,
    string? PrivacyContactEmail,
    string? PrivacyContactPhone,
    string? DpoName,
    string? DpoEmail,
    string? DpoPhone) : IRequest<OrganisationProfileDto>;

public sealed class UpdateOrganisationProfileCommandValidator : AbstractValidator<UpdateOrganisationProfileCommand>
{
    public UpdateOrganisationProfileCommandValidator()
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
        RuleFor(x => x.PrivacyContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.PrivacyContactEmail));
        RuleFor(x => x.DpoEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.DpoEmail));
    }
}

/// <summary>Organisation has no tenant query filter of its own — see GetOrganisationProfileQuery's doc comment for why this handler checks explicitly.</summary>
public sealed class UpdateOrganisationProfileCommandHandler(
    IAppDbContext db,
    ICurrentUserContext currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<UpdateOrganisationProfileCommand, OrganisationProfileDto>
{
    public async Task<OrganisationProfileDto> Handle(UpdateOrganisationProfileCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator && request.OrganisationId != currentUser.OrganisationId)
        {
            throw new NotFoundException(nameof(Organisation), request.OrganisationId);
        }

        var organisation = await db.Organisations
            .Include(o => o.Locations.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == request.OrganisationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Organisation), request.OrganisationId);

        var oldValue = new { organisation.Name, organisation.Industry, organisation.Country };

        organisation.Name = request.Name.Trim();
        organisation.LegalName = request.LegalName;
        organisation.Industry = request.Industry;
        organisation.Size = request.Size is null ? null : Enum.Parse<OrganisationSize>(request.Size);
        organisation.Country = request.Country;
        organisation.Website = request.Website;
        organisation.PrimaryContact = new ContactInfo { Name = request.PrimaryContactName, Email = request.PrimaryContactEmail, Phone = request.PrimaryContactPhone };
        organisation.PrivacyContact = new ContactInfo { Name = request.PrivacyContactName, Email = request.PrivacyContactEmail, Phone = request.PrivacyContactPhone };
        organisation.DpoContact = new ContactInfo { Name = request.DpoName, Email = request.DpoEmail, Phone = request.DpoPhone };

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "organisation.updated",
            nameof(Organisation),
            organisation.Id.ToString(),
            oldValue,
            new { organisation.Name, organisation.Industry, organisation.Country },
            cancellationToken);

        return OrganisationMapper.ToProfileDto(organisation);
    }
}
