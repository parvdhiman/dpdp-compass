using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Domain.Modules.Organisations;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Commands.CreateOrganisationLocation;

public sealed record CreateOrganisationLocationCommand(
    Guid OrganisationId,
    string Label,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    bool IsPrimary) : IRequest<OrganisationLocationDto>;

public sealed class CreateOrganisationLocationCommandValidator : AbstractValidator<CreateOrganisationLocationCommand>
{
    public CreateOrganisationLocationCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
    }
}

/// <summary>Organisation has no tenant query filter of its own — see GetOrganisationProfileQuery's doc comment for why this handler checks explicitly.</summary>
public sealed class CreateOrganisationLocationCommandHandler(
    IAppDbContext db,
    ICurrentUserContext currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<CreateOrganisationLocationCommand, OrganisationLocationDto>
{
    public async Task<OrganisationLocationDto> Handle(CreateOrganisationLocationCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator && request.OrganisationId != currentUser.OrganisationId)
        {
            throw new NotFoundException(nameof(Organisation), request.OrganisationId);
        }

        var organisationExists = await db.Organisations.AnyAsync(o => o.Id == request.OrganisationId, cancellationToken);
        if (!organisationExists)
        {
            throw new NotFoundException(nameof(Organisation), request.OrganisationId);
        }

        if (request.IsPrimary)
        {
            var existingPrimaries = await db.OrganisationLocations
                .Where(l => l.OrganisationId == request.OrganisationId && l.IsPrimary)
                .ToListAsync(cancellationToken);
            foreach (var location in existingPrimaries)
            {
                location.IsPrimary = false;
            }
        }

        var newLocation = new OrganisationLocation
        {
            OrganisationId = request.OrganisationId,
            Label = request.Label.Trim(),
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            IsPrimary = request.IsPrimary,
        };

        db.OrganisationLocations.Add(newLocation);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("organisation.location_added", nameof(OrganisationLocation), newLocation.Id.ToString(), newValue: new { newLocation.Label }, cancellationToken: cancellationToken);

        return OrganisationMapper.ToDto(newLocation);
    }
}
