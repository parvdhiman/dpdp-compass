using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Queries.GetRequirements;

public sealed record GetRequirementsQuery(Guid? LegalReferenceId = null) : IRequest<IReadOnlyList<RequirementDto>>;

public sealed class GetRequirementsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetRequirementsQuery, IReadOnlyList<RequirementDto>>
{
    public async Task<IReadOnlyList<RequirementDto>> Handle(GetRequirementsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Requirements
            .AsNoTracking()
            .Include(r => r.LegalReference)
            .Include(r => r.ControlMappings).ThenInclude(m => m.Control).ThenInclude(c => c.ControlCategory)
            .Include(r => r.ControlMappings).ThenInclude(m => m.Control).ThenInclude(c => c.Questions)
            .AsQueryable();

        if (request.LegalReferenceId is { } legalReferenceId)
        {
            query = query.Where(r => r.LegalReferenceId == legalReferenceId);
        }

        var requirements = await query.OrderBy(r => r.Code).ToListAsync(cancellationToken);

        return requirements.Select(r => new RequirementDto(
                r.Id, r.LegalReferenceId, r.LegalReference.Citation, r.LegalReference.SourceCitation,
                r.Code, r.Title, r.Description, r.ReviewStatus.ToString(),
                r.ControlMappings.Select(m => ComplianceMapper.ToSummaryDto(m.Control)).ToList()))
            .ToList();
    }
}
