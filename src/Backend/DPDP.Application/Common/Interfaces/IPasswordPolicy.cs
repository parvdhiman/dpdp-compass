namespace DPDP.Application.Common.Interfaces;

/// <summary>Policy documented in docs/SECURITY.md — implemented once, referenced by every password-setting validator.</summary>
public interface IPasswordPolicy
{
    /// <summary>Empty when the password satisfies the policy.</summary>
    IReadOnlyList<string> Validate(string password, string? email = null);
}
