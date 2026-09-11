using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Risks.DTOs;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Risks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Risks.Queries;

public sealed record GetRisksQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    string? RiskLevel = null,
    string? Status = null,
    Guid? OwnerUserId = null,
    string SortBy = "score",
    bool SortDescending = true) : IRequest<PagedResult<RiskSummaryDto>>;

public sealed class GetRisksQueryHandler(IAppDbContext db) : IRequestHandler<GetRisksQuery, PagedResult<RiskSummaryDto>>
{
    public async Task<PagedResult<RiskSummaryDto>> Handle(GetRisksQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.Risks
            .AsNoTracking()
            .Include(r => r.Owner)
            .Include(r => r.Findings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(r => r.Title.ToUpper().Contains(term) || r.Description.ToUpper().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.RiskLevel) && Enum.TryParse<RiskLevel>(request.RiskLevel, true, out var riskLevel))
        {
            query = query.Where(r => r.CalculatedRiskLevel == riskLevel);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<RiskStatus>(request.Status, true, out var status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (request.OwnerUserId is { } ownerUserId)
        {
            query = query.Where(r => r.OwnerUserId == ownerUserId);
        }

        query = (request.SortBy.ToLowerInvariant(), request.SortDescending) switch
        {
            ("title", false) => query.OrderBy(r => r.Title),
            ("title", true) => query.OrderByDescending(r => r.Title),
            ("createdat", false) => query.OrderBy(r => r.CreatedAt),
            ("createdat", true) => query.OrderByDescending(r => r.CreatedAt),
            (_, false) => query.OrderBy(r => r.CalculatedRiskScore),
            _ => query.OrderByDescending(r => r.CalculatedRiskScore),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var risks = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = risks.Select(RiskMapper.ToSummaryDto).ToList();

        return new PagedResult<RiskSummaryDto>(items, page, pageSize, totalCount);
    }
}
