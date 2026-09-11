using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataDiscovery.Commands;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using MediatR;

namespace DPDP.Application.Modules.DataDiscovery.Queries;

public sealed record GetDataSourceByIdQuery(Guid Id) : IRequest<DataSourceDetailDto>;

public sealed class GetDataSourceByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetDataSourceByIdQuery, DataSourceDetailDto>
{
    public async Task<DataSourceDetailDto> Handle(GetDataSourceByIdQuery request, CancellationToken cancellationToken)
    {
        var dataSource = await DataSourceLoader.LoadAsync(db, request.Id, cancellationToken);
        return DataDiscoveryMapper.ToDetailDto(dataSource);
    }
}
