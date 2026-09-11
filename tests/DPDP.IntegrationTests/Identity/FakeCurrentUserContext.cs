using DPDP.Application.Common.Interfaces;

namespace DPDP.IntegrationTests.Identity;

public sealed class FakeCurrentUserContext(Guid? organisationId, bool isSuperAdministrator = false) : ICurrentUserContext
{
    public bool IsAuthenticated => true;
    public Guid? UserId => null;
    public Guid? OrganisationId => organisationId;
    public bool IsSuperAdministrator => isSuperAdministrator;
    public IReadOnlySet<string> Permissions => new HashSet<string>();
}
