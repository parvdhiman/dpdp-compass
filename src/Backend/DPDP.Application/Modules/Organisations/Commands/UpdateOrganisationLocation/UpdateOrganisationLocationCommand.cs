using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Domain.Modules.Organisations;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Commands.UpdateOrganisationLocation;

public sealed record UpdateOrganisationLocationCommand(
    Guid LocationId,
    string Label,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    bool IsPrimary) : IRequest<OrganisationLocationDto>;

public sealed class UpdateOrganisationLocationCommandValidator : AbstractValidator<UpdateOrganisationLocationCommand>
{
    public UpdateOrganisationLocationCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateOrganisationLocationCommandHandler(
    IAppDbContext db,
    IAuditLogger auditLogger)
    : IRequestHandler<UpdateOrganisationLocationCommand, OrganisationLocationDto>
{
    public async Task<OrganisationLocationDto> Handle(UpdateOrganisationLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await db.OrganisationLocations
            .FirstOrDefaultAsync(l => l.Id == request.LocationId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganisationLocation), request.LocationId);

        if (request.IsPrimary && !location.IsPrimary)
        {
            var existingPrimaries = await db.OrganisationLocations
                .Where(l => l.OrganisationId == location.OrganisationId && l.IsPrimary && l.Id != location.Id)
                .ToListAsync(cancellationToken);
            foreach (var other in existingPrimaries)
            {
                other.IsPrimary = false;
            }
        }

        location.Label = request.Label.Trim();
        location.AddressLine1 = request.AddressLine1;
        location.AddressLine2 = request.AddressLine2;
        location.City = request.City;
        location.State = request.State;
        location.PostalCode = request.PostalCode;
        location.Country = request.Country;
        location.IsPrimary = request.IsPrimary;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("organisation.location_updated", nameof(OrganisationLocation), location.Id.ToString(), cancellationToken: cancellationToken);

        return OrganisationMapper.ToDto(location);
    }
}
