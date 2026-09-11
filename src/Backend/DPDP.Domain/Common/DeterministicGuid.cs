using System.Security.Cryptography;
using System.Text;

namespace DPDP.Domain.Common;

/// <summary>
/// Produces a stable Guid for a given string, always. Used only to generate
/// EF Core migration seed-data ids (Permissions, Roles, RolePermissions)
/// deterministically, so the seed can be expressed as plain strings instead
/// of a hand-maintained list of random Guid literals, while still producing
/// a migration with fixed, unchanging values. Not cryptographically
/// meaningful — never use this for anything security-sensitive.
/// </summary>
public static class DeterministicGuid
{
    public static Guid Create(string input)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
