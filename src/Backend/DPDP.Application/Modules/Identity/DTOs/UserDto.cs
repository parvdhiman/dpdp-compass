namespace DPDP.Application.Modules.Identity.DTOs;

/// <summary>Never includes PasswordHash, SecurityStamp, or MfaSecret — see docs/SECURITY.md.</summary>
public sealed record UserDto(
    Guid Id,
    Guid? OrganisationId,
    string Email,
    string FullName,
    string? PhoneNumber,
    bool IsActive,
    bool MustChangePassword,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt,
    IReadOnlyList<RoleSummaryDto> Roles);

public sealed record RoleSummaryDto(Guid RoleId, string RoleName);
