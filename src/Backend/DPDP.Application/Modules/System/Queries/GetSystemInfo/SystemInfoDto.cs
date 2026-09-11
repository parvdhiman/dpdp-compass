namespace DPDP.Application.Modules.System.Queries.GetSystemInfo;

/// <summary>
/// Response contract for GET /api/v1/system/info. Contains no secrets,
/// connection strings, or internal configuration — see docs/SECURITY.md
/// section 9 ("never expose") and docs/API.md.
/// </summary>
public sealed record SystemInfoDto(
    string ApplicationName,
    string Version,
    string Environment,
    DateTimeOffset ServerTimeUtc);
