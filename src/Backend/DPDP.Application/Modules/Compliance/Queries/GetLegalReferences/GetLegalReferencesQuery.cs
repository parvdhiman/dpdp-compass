using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Queries.GetLegalReferences;

public sealed record GetLegalReferencesQuery(Guid? FrameworkVersionId = null) : IRequest<IReadOnlyList<LegalReferenceDto>>;

public sealed class GetLegalReferencesQueryHandler(IAppDbContext db)
    : IRequestHandler<GetLegalReferencesQuery, IReadOnlyList<LegalReferenceDto>>
{
    public async Task<IReadOnlyList<LegalReferenceDto>> Handle(GetLegalReferencesQuery request, CancellationToken cancellationToken)
    {
        var query = db.LegalReferences.AsNoTracking().Include(l => l.Requirements).AsQueryable();

        if (request.FrameworkVersionId is { } versionId)
        {
            query = query.Where(l => l.FrameworkVersionId == versionId);
        }

        var legalReferences = await query.OrderBy(l => l.Citation).ToListAsync(cancellationToken);
        return legalReferences.Select(ComplianceMapper.ToDto).ToList();
    }
}
