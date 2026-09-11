using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Domain.Common;
using DPDP.Domain.Modules.Organisations;

namespace DPDP.Application.Modules.Organisations;

/// <summary>Shared entity→DTO mapping so Get/Create/Update handlers can't drift from each other.</summary>
internal static class OrganisationMapper
{
    public static ContactInfoDto ToDto(ContactInfo? contact) =>
        new(contact?.Name, contact?.Email, contact?.Phone);

    public static OrganisationLocationDto ToDto(OrganisationLocation location) => new(
        location.Id,
        location.Label,
        location.AddressLine1,
        location.AddressLine2,
        location.City,
        location.State,
        location.PostalCode,
        location.Country,
        location.IsPrimary);

    public static OrganisationProfileDto ToProfileDto(Organisation organisation) => new(
        organisation.Id,
        organisation.Name,
        organisation.LegalName,
        organisation.Status.ToString(),
        organisation.Industry,
        organisation.Size?.ToString(),
        organisation.Country,
        organisation.Website,
        ToDto(organisation.PrimaryContact),
        ToDto(organisation.PrivacyContact),
        ToDto(organisation.DpoContact),
        organisation.Locations.Select(ToDto).ToList(),
        organisation.CreatedAt);
}
