using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Evidence.Commands;
using DPDP.Application.Modules.Evidence.DTOs;
using MediatR;

namespace DPDP.Application.Modules.Evidence.Queries;

public sealed record GetEvidenceByIdQuery(Guid Id) : IRequest<EvidenceDetailDto>;

public sealed class GetEvidenceByIdQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetEvidenceByIdQuery, EvidenceDetailDto>
{
    public async Task<EvidenceDetailDto> Handle(GetEvidenceByIdQuery request, CancellationToken cancellationToken)
    {
        var evidence = await EvidenceLoader.LoadForDetailAsync(db, request.Id, cancellationToken);
        return EvidenceMapper.ToDetailDto(evidence, DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime));
    }
}
