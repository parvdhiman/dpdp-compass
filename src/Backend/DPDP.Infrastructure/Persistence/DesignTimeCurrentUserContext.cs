using DPDP.Application.Common.Interfaces;

namespace DPDP.Infrastructure.Persistence;

/// <summary>
/// Used only by DpdpDbContextFactory for `dotnet ef` CLI commands, where
/// there is no HTTP request to resolve a real ICurrentUserContext from.
/// IsSuperAdministrator=true makes every query filter a no-op, which is
/// correct for schema/migration tooling — never register this for
/// application runtime use.
/// </summary>
public sealed class DesignTimeCurrentUserContext : ICurrentUserContext
{
    public bool IsAuthenticated => false;
    public Guid? UserId => null;
    public Guid? OrganisationId => null;
    public bool IsSuperAdministrator => true;
    public IReadOnlySet<string> Permissions => new HashSet<string>();
}
