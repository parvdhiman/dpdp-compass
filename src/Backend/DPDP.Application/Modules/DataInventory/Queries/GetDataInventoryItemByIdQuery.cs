using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataInventory.Commands;
using DPDP.Application.Modules.DataInventory.DTOs;
using MediatR;

namespace DPDP.Application.Modules.DataInventory.Queries;

public sealed record GetDataInventoryItemByIdQuery(Guid Id) : IRequest<DataInventoryItemDto>;

public sealed class GetDataInventoryItemByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetDataInventoryItemByIdQuery, DataInventoryItemDto>
{
    public async Task<DataInventoryItemDto> Handle(GetDataInventoryItemByIdQuery request, CancellationToken cancellationToken) =>
        DataInventoryMapper.ToDto(await DataInventoryItemLoader.LoadForDetailAsync(db, request.Id, cancellationToken));
}
