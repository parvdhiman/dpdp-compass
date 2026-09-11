using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataInventory.Commands;
using DPDP.Application.Modules.DataInventory.DTOs;
using MediatR;

namespace DPDP.Application.Modules.DataInventory.Queries;

public sealed record GetProcessingActivityByIdQuery(Guid Id) : IRequest<ProcessingActivityDetailDto>;

public sealed class GetProcessingActivityByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetProcessingActivityByIdQuery, ProcessingActivityDetailDto>
{
    public async Task<ProcessingActivityDetailDto> Handle(GetProcessingActivityByIdQuery request, CancellationToken cancellationToken) =>
        DataInventoryMapper.ToDetailDto(await ProcessingActivityLoader.LoadForDetailAsync(db, request.Id, cancellationToken));
}
