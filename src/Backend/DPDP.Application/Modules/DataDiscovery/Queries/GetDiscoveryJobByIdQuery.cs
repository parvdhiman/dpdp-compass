using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataDiscovery.Commands;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using MediatR;

namespace DPDP.Application.Modules.DataDiscovery.Queries;

public sealed record GetDiscoveryJobByIdQuery(Guid Id) : IRequest<DiscoveryJobDetailDto>;

public sealed class GetDiscoveryJobByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetDiscoveryJobByIdQuery, DiscoveryJobDetailDto>
{
    public async Task<DiscoveryJobDetailDto> Handle(GetDiscoveryJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await DiscoveryJobLoader.LoadForDetailAsync(db, request.Id, cancellationToken);
        return DataDiscoveryMapper.ToDetailDto(job);
    }
}
