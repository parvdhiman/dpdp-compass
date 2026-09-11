using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Identity;
using Microsoft.AspNetCore.Identity;

namespace DPDP.Infrastructure.Security;

/// <summary>
/// Wraps ASP.NET Core Identity's PasswordHasher&lt;T&gt; (PBKDF2) as a
/// standalone utility — this project does not use the full ASP.NET Core
/// Identity system (UserManager/SignInManager/its own schema); User is a
/// plain entity per docs/DATABASE.md. See docs/ARCHITECTURE.md section 11.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _inner = new();

    public string HashPassword(string password) => _inner.HashPassword(null!, password);

    public bool VerifyPassword(string hashedPassword, string providedPassword) =>
        _inner.VerifyHashedPassword(null!, hashedPassword, providedPassword)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}
