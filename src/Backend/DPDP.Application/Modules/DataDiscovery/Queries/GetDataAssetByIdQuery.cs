using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataDiscovery.Commands;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using MediatR;

namespace DPDP.Application.Modules.DataDiscovery.Queries;

public sealed record GetDataAssetByIdQuery(Guid Id) : IRequest<DataAssetDetailDto>;

public sealed class GetDataAssetByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetDataAssetByIdQuery, DataAssetDetailDto>
{
    public async Task<DataAssetDetailDto> Handle(GetDataAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var asset = await DataAssetLoader.LoadForDetailAsync(db, request.Id, cancellationToken);
        return DataDiscoveryMapper.ToDetailDto(asset);
    }
}
