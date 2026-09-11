namespace DPDP.Application.Modules.Identity.DTOs;

public sealed record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystemRole,
    IReadOnlyList<PermissionDto> Permissions);

public sealed record PermissionDto(Guid Id, string Key, string Description, string Module);
