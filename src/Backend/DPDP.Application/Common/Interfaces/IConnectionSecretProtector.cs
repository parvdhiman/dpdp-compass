namespace DPDP.Application.Common.Interfaces;

/// <summary>
/// Encrypts/decrypts a DataSource's connection secret (password/key) at
/// rest. The only implementation (ASP.NET Core Data Protection) lives in
/// Infrastructure — see docs/DATA_DISCOVERY.md section 2. Never log
/// either argument or return value of Unprotect.
/// </summary>
public interface IConnectionSecretProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedValue);
}
