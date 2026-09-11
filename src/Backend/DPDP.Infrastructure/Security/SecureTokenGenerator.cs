using System.Security.Cryptography;
using DPDP.Application.Common.Interfaces;

namespace DPDP.Infrastructure.Security;

public sealed class SecureTokenGenerator : ISecureTokenGenerator
{
    public string GenerateToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    public string Hash(string token) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
}
