using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Queries.GetFrameworks;

public sealed record GetFrameworksQuery : IRequest<IReadOnlyList<FrameworkDto>>;

public sealed class GetFrameworksQueryHandler(IAppDbContext db) : IRequestHandler<GetFrameworksQuery, IReadOnlyList<FrameworkDto>>
{
    public async Task<IReadOnlyList<FrameworkDto>> Handle(GetFrameworksQuery request, CancellationToken cancellationToken)
    {
        var frameworks = await db.Frameworks
            .AsNoTracking()
            .Include(f => f.Versions)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);

        return frameworks.Select(ComplianceMapper.ToDto).ToList();
    }
}
