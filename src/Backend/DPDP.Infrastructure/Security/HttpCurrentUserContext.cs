using System.Security.Claims;
using DPDP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DPDP.Infrastructure.Security;

public sealed class HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public Guid? OrganisationId =>
        Guid.TryParse(Principal?.FindFirstValue("org_id"), out var id) ? id : null;

    public bool IsSuperAdministrator =>
        string.Equals(Principal?.FindFirstValue("is_super_admin"), "true", StringComparison.OrdinalIgnoreCase);

    public IReadOnlySet<string> Permissions =>
        Principal?.FindAll("permission").Select(c => c.Value).ToHashSet() ?? new HashSet<string>();
}
