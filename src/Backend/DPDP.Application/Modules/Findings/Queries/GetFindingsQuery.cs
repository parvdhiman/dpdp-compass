using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Findings.DTOs;
using DPDP.Domain.Modules.Findings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Findings.Queries;

/// <summary>Backs the Finding Dashboard.</summary>
public sealed record GetFindingsQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    string? Status = null,
    string? Severity = null,
    Guid? OwnerUserId = null,
    Guid? RiskId = null,
    bool? OverdueOnly = null,
    string SortBy = "createdAt",
    bool SortDescending = true) : IRequest<PagedResult<FindingSummaryDto>>;

public sealed class GetFindingsQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetFindingsQuery, PagedResult<FindingSummaryDto>>
{
    public async Task<PagedResult<FindingSummaryDto>> Handle(GetFindingsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);

        var query = db.Findings
            .AsNoTracking()
            .Include(f => f.Owner)
            .Include(f => f.Risk)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(f => f.Title.ToUpper().Contains(term) || f.Description.ToUpper().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<FindingStatus>(request.Status, true, out var status))
        {
            query = query.Where(f => f.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Severity) && Enum.TryParse<FindingSeverity>(request.Severity, true, out var severity))
        {
            query = query.Where(f => f.Severity == severity);
        }

        if (request.OwnerUserId is { } ownerUserId)
        {
            query = query.Where(f => f.OwnerUserId == ownerUserId);
        }

        if (request.RiskId is { } riskId)
        {
            query = query.Where(f => f.RiskId == riskId);
        }

        if (request.OverdueOnly == true)
        {
            var terminal = new[] { FindingStatus.RESOLVED, FindingStatus.CLOSED, FindingStatus.ACCEPTED_RISK };
            query = query.Where(f => f.DueDate != null && f.DueDate < today && !terminal.Contains(f.Status));
        }

        query = (request.SortBy.ToLowerInvariant(), request.SortDescending) switch
        {
            ("title", false) => query.OrderBy(f => f.Title),
            ("title", true) => query.OrderByDescending(f => f.Title),
            ("severity", false) => query.OrderBy(f => f.Severity),
            ("severity", true) => query.OrderByDescending(f => f.Severity),
            ("duedate", false) => query.OrderBy(f => f.DueDate),
            ("duedate", true) => query.OrderByDescending(f => f.DueDate),
            (_, false) => query.OrderBy(f => f.CreatedAt),
            _ => query.OrderByDescending(f => f.CreatedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var findings = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = findings.Select(f => FindingMapper.ToSummaryDto(f, today)).ToList();

        return new PagedResult<FindingSummaryDto>(items, page, pageSize, totalCount);
    }
}
