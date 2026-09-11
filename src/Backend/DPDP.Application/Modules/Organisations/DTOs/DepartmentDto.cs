namespace DPDP.Application.Modules.Organisations.DTOs;

public sealed record DepartmentDto(
    Guid Id,
    Guid OrganisationId,
    Guid BusinessUnitId,
    string BusinessUnitName,
    string Name,
    string? Description,
    ContactInfoDto Head,
    bool IsActive,
    DateTimeOffset CreatedAt);
