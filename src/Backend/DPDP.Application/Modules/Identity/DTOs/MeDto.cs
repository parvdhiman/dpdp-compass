namespace DPDP.Application.Modules.Identity.DTOs;

public sealed record MeDto(
    Guid Id,
    Guid? OrganisationId,
    string Email,
    string FullName,
    bool IsSuperAdministrator,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
