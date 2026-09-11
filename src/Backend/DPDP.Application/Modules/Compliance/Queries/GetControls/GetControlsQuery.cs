using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Queries.GetControls;

/// <summary>
/// Serves both the admin control library view and the organisation-facing
/// read-only view from one query: a non-Super-Administrator caller always
/// gets ControlStatus.ACTIVE only, regardless of what status filter (if
/// any) was requested — see docs/COMPLIANCE_CONTENT_GOVERNANCE.md. This
/// avoids needing two near-duplicate endpoints/handlers.
/// </summary>
public sealed record GetControlsQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    Guid? CategoryId = null,
    string? RiskLevel = null,
    string? Status = null,
    Guid? FrameworkVersionId = null,
    bool? HasApplicableConditions = null,
    string SortBy = "controlId",
    bool SortDescending = false) : IRequest<PagedResult<ControlSummaryDto>>;

public sealed class GetControlsQueryHandler(IAppDbContext db, ICurrentUserContext currentUser)
    : IRequestHandler<GetControlsQuery, PagedResult<ControlSummaryDto>>
{
    public async Task<PagedResult<ControlSummaryDto>> Handle(GetControlsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.Controls
            .AsNoTracking()
            .Include(c => c.ControlCategory)
            .Include(c => c.Questions)
            .AsQueryable();

        if (currentUser.IsSuperAdministrator)
        {
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ControlStatus>(request.Status, true, out var status))
            {
                query = query.Where(c => c.Status == status);
            }
        }
        else
        {
            // Organisation-facing visibility rule: only published, operational controls.
            query = query.Where(c => c.Status == ControlStatus.ACTIVE);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(c =>
                c.Name.ToUpper().Contains(term) ||
                c.ControlId.ToUpper().Contains(term) ||
                c.Description.ToUpper().Contains(term) ||
                (c.ApplicableConditions != null && c.ApplicableConditions.ToUpper().Contains(term)));
        }

        if (request.CategoryId is { } categoryId)
        {
            query = query.Where(c => c.ControlCategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(request.RiskLevel) && Enum.TryParse<RiskLevel>(request.RiskLevel, true, out var riskLevel))
        {
            query = query.Where(c => c.RiskLevel == riskLevel);
        }

        if (request.HasApplicableConditions is { } hasConditions)
        {
            query = hasConditions
                ? query.Where(c => c.ApplicableConditions != null)
                : query.Where(c => c.ApplicableConditions == null);
        }

        if (request.FrameworkVersionId is { } frameworkVersionId)
        {
            query = query.Where(c => c.ControlMappings.Any(m => m.Requirement.LegalReference.FrameworkVersionId == frameworkVersionId));
        }

        query = (request.SortBy.ToLowerInvariant(), request.SortDescending) switch
        {
            ("name", false) => query.OrderBy(c => c.Name),
            ("name", true) => query.OrderByDescending(c => c.Name),
            ("risklevel", false) => query.OrderBy(c => c.RiskLevel),
            ("risklevel", true) => query.OrderByDescending(c => c.RiskLevel),
            (_, true) => query.OrderByDescending(c => c.ControlId),
            _ => query.OrderBy(c => c.ControlId),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var controls = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = controls.Select(ComplianceMapper.ToSummaryDto).ToList();

        return new PagedResult<ControlSummaryDto>(items, page, pageSize, totalCount);
    }
}
