using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Risks.DTOs;
using DPDP.Domain.Modules.Risks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Risks.Queries;

public sealed record GetRiskByIdQuery(Guid Id) : IRequest<RiskDetailDto>;

public sealed class GetRiskByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetRiskByIdQuery, RiskDetailDto>
{
    public async Task<RiskDetailDto> Handle(GetRiskByIdQuery request, CancellationToken cancellationToken)
    {
        var risk = await db.Risks
            .AsNoTracking()
            .Include(r => r.Owner)
            .Include(r => r.Findings)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Risk), request.Id);

        return RiskMapper.ToDetailDto(risk);
    }
}
