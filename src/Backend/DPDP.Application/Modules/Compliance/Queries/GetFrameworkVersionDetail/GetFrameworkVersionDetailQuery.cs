using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Queries.GetFrameworkVersionDetail;

public sealed record GetFrameworkVersionDetailQuery(Guid Id) : IRequest<FrameworkVersionDto>;

public sealed class GetFrameworkVersionDetailQueryHandler(IAppDbContext db)
    : IRequestHandler<GetFrameworkVersionDetailQuery, FrameworkVersionDto>
{
    public async Task<FrameworkVersionDto> Handle(GetFrameworkVersionDetailQuery request, CancellationToken cancellationToken)
    {
        var version = await db.FrameworkVersions
            .AsNoTracking()
            .Include(v => v.Framework)
            .Include(v => v.LegalReferences).ThenInclude(l => l.Requirements)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FrameworkVersion), request.Id);

        return ComplianceMapper.ToDetailDto(version);
    }
}
