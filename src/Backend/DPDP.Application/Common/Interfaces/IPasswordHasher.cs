namespace DPDP.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);

    /// <summary>True if the plaintext password matches the stored hash. Never logs either value.</summary>
    bool VerifyPassword(string hashedPassword, string providedPassword);
}
