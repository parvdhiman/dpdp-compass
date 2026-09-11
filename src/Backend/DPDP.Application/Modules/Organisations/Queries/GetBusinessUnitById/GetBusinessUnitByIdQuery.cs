using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Domain.Modules.Organisations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Queries.GetBusinessUnitById;

public sealed record GetBusinessUnitByIdQuery(Guid Id) : IRequest<BusinessUnitDto>;

public sealed class GetBusinessUnitByIdQueryHandler(IAppDbContext db)
    : IRequestHandler<GetBusinessUnitByIdQuery, BusinessUnitDto>
{
    public async Task<BusinessUnitDto> Handle(GetBusinessUnitByIdQuery request, CancellationToken cancellationToken)
    {
        var businessUnit = await db.BusinessUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(BusinessUnit), request.Id);

        var departmentCount = await db.Departments.CountAsync(d => d.BusinessUnitId == businessUnit.Id, cancellationToken);

        return new BusinessUnitDto(
            businessUnit.Id, businessUnit.OrganisationId, businessUnit.Name, businessUnit.Description,
            OrganisationMapper.ToDto(businessUnit.Head), businessUnit.IsActive, departmentCount, businessUnit.CreatedAt);
    }
}
