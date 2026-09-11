using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Domain.Modules.Organisations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Queries.GetOrganisationDashboard;

public sealed record GetOrganisationDashboardQuery(Guid OrganisationId) : IRequest<OrganisationDashboardDto>;

/// <summary>Organisation has no tenant query filter of its own — see GetOrganisationProfileQuery's doc comment for why this handler checks explicitly.</summary>
public sealed class GetOrganisationDashboardQueryHandler(IAppDbContext db, ICurrentUserContext currentUser)
    : IRequestHandler<GetOrganisationDashboardQuery, OrganisationDashboardDto>
{
    public async Task<OrganisationDashboardDto> Handle(GetOrganisationDashboardQuery request, CancellationToken cancellationToken)
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

        var businessUnitCount = await db.BusinessUnits.CountAsync(b => b.OrganisationId == request.OrganisationId, cancellationToken);
        var departmentCount = await db.Departments.CountAsync(d => d.OrganisationId == request.OrganisationId, cancellationToken);
        var activeUserCount = await db.Users.CountAsync(u => u.OrganisationId == request.OrganisationId && u.IsActive, cancellationToken);

        var primaryLocation = organisation.Locations.FirstOrDefault(l => l.IsPrimary) ?? organisation.Locations.FirstOrDefault();
        var hasDpoConfigured = !string.IsNullOrWhiteSpace(organisation.DpoContact.Email);

        return new OrganisationDashboardDto(
            organisation.Id,
            organisation.Name,
            organisation.Industry,
            organisation.Size?.ToString(),
            businessUnitCount,
            departmentCount,
            activeUserCount,
            primaryLocation is null ? null : OrganisationMapper.ToDto(primaryLocation),
            hasDpoConfigured);
    }
}
