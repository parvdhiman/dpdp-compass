using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Evidence.DTOs;
using DPDP.Domain.Modules.Evidence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Evidence.Queries;

/// <summary>Backs the Evidence Dashboard and the "Evidence" tab on Assessment/Control/Finding detail pages.</summary>
public sealed record GetEvidenceListQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    string? Status = null,
    string? EvidenceType = null,
    Guid? OwnerUserId = null,
    Guid? ReviewerUserId = null,
    Guid? AssessmentId = null,
    Guid? ControlId = null,
    Guid? FindingId = null,
    bool? ExpiredOnly = null,
    string SortBy = "createdAt",
    bool SortDescending = true) : IRequest<PagedResult<EvidenceSummaryDto>>;

public sealed class GetEvidenceListQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetEvidenceListQuery, PagedResult<EvidenceSummaryDto>>
{
    public async Task<PagedResult<EvidenceSummaryDto>> Handle(GetEvidenceListQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);

        var query = db.EvidenceItems
            .AsNoTracking()
            .Include(e => e.Owner)
            .Include(e => e.Reviewer)
            .Include(e => e.Versions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(e => e.Title.ToUpper().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<EvidenceStatus>(request.Status, true, out var status))
        {
            query = query.Where(e => e.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.EvidenceType) && Enum.TryParse<Domain.Modules.Evidence.EvidenceType>(request.EvidenceType, true, out var evidenceType))
        {
            query = query.Where(e => e.EvidenceType == evidenceType);
        }

        if (request.OwnerUserId is { } ownerUserId)
        {
            query = query.Where(e => e.OwnerUserId == ownerUserId);
        }

        if (request.ReviewerUserId is { } reviewerUserId)
        {
            query = query.Where(e => e.ReviewerUserId == reviewerUserId);
        }

        if (request.AssessmentId is { } assessmentId)
        {
            query = query.Where(e => e.AssessmentId == assessmentId);
        }

        if (request.ControlId is { } controlId)
        {
            query = query.Where(e => e.ControlId == controlId);
        }

        if (request.FindingId is { } findingId)
        {
            query = query.Where(e => e.FindingId == findingId);
        }

        if (request.ExpiredOnly == true)
        {
            query = query.Where(e => e.ExpiryDate != null && e.ExpiryDate < today && (e.Status == EvidenceStatus.APPROVED || e.Status == EvidenceStatus.EXPIRED));
        }

        query = (request.SortBy.ToLowerInvariant(), request.SortDescending) switch
        {
            ("title", false) => query.OrderBy(e => e.Title),
            ("title", true) => query.OrderByDescending(e => e.Title),
            ("expirydate", false) => query.OrderBy(e => e.ExpiryDate),
            ("expirydate", true) => query.OrderByDescending(e => e.ExpiryDate),
            ("status", false) => query.OrderBy(e => e.Status),
            ("status", true) => query.OrderByDescending(e => e.Status),
            (_, false) => query.OrderBy(e => e.CreatedAt),
            _ => query.OrderByDescending(e => e.CreatedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var evidenceItems = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = evidenceItems.Select(e => EvidenceMapper.ToSummaryDto(e, today)).ToList();

        return new PagedResult<EvidenceSummaryDto>(items, page, pageSize, totalCount);
    }
}
