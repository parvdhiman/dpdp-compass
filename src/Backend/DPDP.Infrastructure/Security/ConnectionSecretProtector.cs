using DPDP.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace DPDP.Infrastructure.Security;

/// <summary>
/// ASP.NET Core Data Protection, not a hand-rolled cipher — key
/// management/rotation is handled by the framework. See
/// DependencyInjection.AddInfrastructure for where keys are persisted and
/// docs/DATA_DISCOVERY.md section 2 for the full rationale.
/// </summary>
public sealed class ConnectionSecretProtector : IConnectionSecretProtector
{
    private const string Purpose = "DPDP.DataDiscovery.ConnectionSecrets.v1";

    private readonly IDataProtector _protector;

    public ConnectionSecretProtector(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string protectedValue) => _protector.Unprotect(protectedValue);
}
