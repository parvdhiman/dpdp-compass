namespace DPDP.Application.Common.Interfaces;

/// <summary>
/// Resolved once per request from the JWT's claims. DpdpDbContext's global
/// query filters depend on this — see docs/ARCHITECTURE.md section 4.
/// Never read claims directly anywhere else; always go through this.
/// </summary>
public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    Guid? OrganisationId { get; }
    bool IsSuperAdministrator { get; }
    IReadOnlySet<string> Permissions { get; }
}
