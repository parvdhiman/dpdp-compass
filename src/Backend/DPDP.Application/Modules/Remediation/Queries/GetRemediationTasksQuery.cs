using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Remediation.DTOs;
using DPDP.Domain.Modules.Remediation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Remediation.Queries;

/// <summary>Backs both the Remediation Dashboard (no filter) and the Overdue Tasks page (OverdueOnly=true).</summary>
public sealed record GetRemediationTasksQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    string? Status = null,
    Guid? OwnerUserId = null,
    Guid? FindingId = null,
    bool? OverdueOnly = null,
    string SortBy = "dueDate",
    bool SortDescending = false) : IRequest<PagedResult<RemediationTaskSummaryDto>>;

public sealed class GetRemediationTasksQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetRemediationTasksQuery, PagedResult<RemediationTaskSummaryDto>>
{
    public async Task<PagedResult<RemediationTaskSummaryDto>> Handle(GetRemediationTasksQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);

        var query = db.RemediationTasks
            .AsNoTracking()
            .Include(t => t.Finding)
            .Include(t => t.Owner)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(t => t.Title.ToUpper().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<RemediationStatus>(request.Status, true, out var status))
        {
            query = query.Where(t => t.Status == status);
        }

        if (request.OwnerUserId is { } ownerUserId)
        {
            query = query.Where(t => t.OwnerUserId == ownerUserId);
        }

        if (request.FindingId is { } findingId)
        {
            query = query.Where(t => t.FindingId == findingId);
        }

        if (request.OverdueOnly == true)
        {
            var terminal = new[] { RemediationStatus.VERIFIED, RemediationStatus.CLOSED };
            query = query.Where(t => t.DueDate != null && t.DueDate < today && !terminal.Contains(t.Status));
        }

        query = (request.SortBy.ToLowerInvariant(), request.SortDescending) switch
        {
            ("title", false) => query.OrderBy(t => t.Title),
            ("title", true) => query.OrderByDescending(t => t.Title),
            ("createdat", false) => query.OrderBy(t => t.CreatedAt),
            ("createdat", true) => query.OrderByDescending(t => t.CreatedAt),
            (_, true) => query.OrderByDescending(t => t.DueDate),
            _ => query.OrderBy(t => t.DueDate),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var tasks = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = tasks.Select(t => RemediationMapper.ToSummaryDto(t, today)).ToList();

        return new PagedResult<RemediationTaskSummaryDto>(items, page, pageSize, totalCount);
    }
}
