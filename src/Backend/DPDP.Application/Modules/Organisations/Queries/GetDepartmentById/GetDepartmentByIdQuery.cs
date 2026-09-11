using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Organisations.DTOs;
using DPDP.Domain.Modules.Organisations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Organisations.Queries.GetDepartmentById;

public sealed record GetDepartmentByIdQuery(Guid Id) : IRequest<DepartmentDto>;

public sealed class GetDepartmentByIdQueryHandler(IAppDbContext db)
    : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto>
{
    public async Task<DepartmentDto> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = await db.Departments
            .AsNoTracking()
            .Include(d => d.BusinessUnit)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Department), request.Id);

        return new DepartmentDto(
            department.Id, department.OrganisationId, department.BusinessUnitId, department.BusinessUnit.Name,
            department.Name, department.Description, OrganisationMapper.ToDto(department.Head), department.IsActive, department.CreatedAt);
    }
}
