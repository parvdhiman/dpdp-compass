using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Findings.DTOs;
using DPDP.Domain.Modules.Findings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Findings.Queries;

public sealed record GetFindingByIdQuery(Guid Id) : IRequest<FindingDetailDto>;

public sealed class GetFindingByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetFindingByIdQuery, FindingDetailDto>
{
    public async Task<FindingDetailDto> Handle(GetFindingByIdQuery request, CancellationToken cancellationToken)
    {
        var finding = await db.Findings
            .AsNoTracking()
            .Include(f => f.Control)
            .Include(f => f.Risk)
            .Include(f => f.Owner)
            .Include(f => f.RemediationTasks).ThenInclude(t => t.Owner)
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Finding), request.Id);

        return FindingMapper.ToDetailDto(finding);
    }
}
