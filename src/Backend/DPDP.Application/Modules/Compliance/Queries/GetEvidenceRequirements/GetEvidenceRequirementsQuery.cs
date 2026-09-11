using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Queries.GetEvidenceRequirements;

public sealed record GetEvidenceRequirementsQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    Guid? AssessmentQuestionId = null) : IRequest<PagedResult<EvidenceRequirementDto>>;

public sealed class GetEvidenceRequirementsQueryHandler(IAppDbContext db, ICurrentUserContext currentUser)
    : IRequestHandler<GetEvidenceRequirementsQuery, PagedResult<EvidenceRequirementDto>>
{
    public async Task<PagedResult<EvidenceRequirementDto>> Handle(GetEvidenceRequirementsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.EvidenceRequirements
            .AsNoTracking()
            .Include(e => e.AssessmentQuestion).ThenInclude(q => q.Control)
            .AsQueryable();

        if (!currentUser.IsSuperAdministrator)
        {
            query = query.Where(e => e.AssessmentQuestion.Control.Status == ControlStatus.ACTIVE);
        }

        if (request.AssessmentQuestionId is { } questionId)
        {
            query = query.Where(e => e.AssessmentQuestionId == questionId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(e => e.Name.ToUpper().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var evidence = await query
            .OrderBy(e => e.AssessmentQuestion.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = evidence.Select(ComplianceMapper.ToDto).ToList();
        return new PagedResult<EvidenceRequirementDto>(items, page, pageSize, totalCount);
    }
}
