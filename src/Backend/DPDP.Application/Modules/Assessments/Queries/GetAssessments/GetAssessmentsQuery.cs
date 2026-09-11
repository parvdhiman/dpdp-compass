using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Assessments.Queries.GetAssessments;

/// <summary>Tenant-scoped entirely by Assessment's own EF query filter — no explicit organisation check needed here (see docs/ARCHITECTURE.md Module 5 section).</summary>
public sealed record GetAssessmentsQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    string? Status = null,
    Guid? FrameworkVersionId = null,
    Guid? AssignedToUserId = null,
    string SortBy = "createdAt",
    bool SortDescending = true) : IRequest<PagedResult<AssessmentSummaryDto>>;

public sealed class GetAssessmentsQueryHandler(IAppDbContext db) : IRequestHandler<GetAssessmentsQuery, PagedResult<AssessmentSummaryDto>>
{
    public async Task<PagedResult<AssessmentSummaryDto>> Handle(GetAssessmentsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.Assessments
            .AsNoTracking()
            .Include(a => a.FrameworkVersion).ThenInclude(v => v.Framework)
            .Include(a => a.AssignedToUser)
            .Include(a => a.Controls)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(a => a.Name.ToUpper().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<AssessmentStatus>(request.Status, true, out var status))
        {
            query = query.Where(a => a.Status == status);
        }

        if (request.FrameworkVersionId is { } frameworkVersionId)
        {
            query = query.Where(a => a.FrameworkVersionId == frameworkVersionId);
        }

        if (request.AssignedToUserId is { } assignedToUserId)
        {
            query = query.Where(a => a.AssignedToUserId == assignedToUserId);
        }

        query = (request.SortBy.ToLowerInvariant(), request.SortDescending) switch
        {
            ("name", false) => query.OrderBy(a => a.Name),
            ("name", true) => query.OrderByDescending(a => a.Name),
            ("duedate", false) => query.OrderBy(a => a.DueDate),
            ("duedate", true) => query.OrderByDescending(a => a.DueDate),
            ("status", false) => query.OrderBy(a => a.Status),
            ("status", true) => query.OrderByDescending(a => a.Status),
            (_, false) => query.OrderBy(a => a.CreatedAt),
            _ => query.OrderByDescending(a => a.CreatedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var assessments = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = assessments.Select(AssessmentMapper.ToSummaryDto).ToList();

        return new PagedResult<AssessmentSummaryDto>(items, page, pageSize, totalCount);
    }
}
