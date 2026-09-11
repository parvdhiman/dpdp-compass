namespace DPDP.Application.Modules.Identity.DTOs;

public sealed record AuthResultDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    MeDto User);
