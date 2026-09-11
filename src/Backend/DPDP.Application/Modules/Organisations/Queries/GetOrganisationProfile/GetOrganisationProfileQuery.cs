using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Domain.Modules.Organisations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Queries.GetOrganisationProfile;

public sealed record GetOrganisationProfileQuery(Guid OrganisationId) : IRequest<OrganisationProfileDto>;

/// <summary>
/// Organisation IS the tenant, so — unlike User/BusinessUnit/Department —
/// it has no ITenantScoped query filter to fall back on (there is nothing
/// to compare its own id against). Every handler that loads an
/// Organisation by id must therefore check
/// currentUser.IsSuperAdministrator || organisation.Id == currentUser.OrganisationId
/// explicitly, itself. This was missed on the first pass and caught by
/// DPDP.ApiTests.Organisations.OrganisationManagementApiTests — see the
/// Module 3 completion report's Known Issues.
/// </summary>
public sealed class GetOrganisationProfileQueryHandler(IAppDbContext db, ICurrentUserContext currentUser)
    : IRequestHandler<GetOrganisationProfileQuery, OrganisationProfileDto>
{
    public async Task<OrganisationProfileDto> Handle(GetOrganisationProfileQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator && request.OrganisationId != currentUser.OrganisationId)
        {
            throw new NotFoundException(nameof(Organisation), request.OrganisationId);
        }

        var organisation = await db.Organisations
            .AsNoTracking()
            .Include(o => o.Locations.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == request.OrganisationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Organisation), request.OrganisationId);

        return OrganisationMapper.ToProfileDto(organisation);
    }
}
