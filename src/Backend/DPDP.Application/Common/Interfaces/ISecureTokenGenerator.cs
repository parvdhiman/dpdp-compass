namespace DPDP.Application.Common.Interfaces;

/// <summary>
/// Generates opaque, cryptographically random tokens (refresh tokens,
/// password reset tokens) and hashes them for storage — the raw value is
/// never persisted or logged, only returned once to the caller.
/// </summary>
public interface ISecureTokenGenerator
{
    string GenerateToken();
    string Hash(string token);
}
