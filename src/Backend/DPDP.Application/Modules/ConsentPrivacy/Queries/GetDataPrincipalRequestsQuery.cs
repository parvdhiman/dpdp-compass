using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.ConsentPrivacy.Commands;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Domain.Modules.ConsentPrivacy;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.ConsentPrivacy.Queries;

public sealed record GetDataPrincipalRequestsQuery(
    int Page = 1, int PageSize = 25, string? Status = null, string? RequestType = null,
    Guid? AssignedToUserId = null, bool? OverdueOnly = null)
    : IRequest<PagedResult<DataPrincipalRequestSummaryDto>>;

public sealed class GetDataPrincipalRequestsQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetDataPrincipalRequestsQuery, PagedResult<DataPrincipalRequestSummaryDto>>
{
    public async Task<PagedResult<DataPrincipalRequestSummaryDto>> Handle(GetDataPrincipalRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = db.DataPrincipalRequests.AsNoTracking().Include(r => r.AssignedToUser).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<DataPrincipalRequestStatus>(request.Status, true, out var status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.RequestType) && Enum.TryParse<DataPrincipalRequestType>(request.RequestType, true, out var type))
        {
            query = query.Where(r => r.RequestType == type);
        }

        if (request.AssignedToUserId is { } assignedToUserId) query = query.Where(r => r.AssignedToUserId == assignedToUserId);

        if (request.OverdueOnly == true)
        {
            var now = dateTimeProvider.UtcNow;
            query = query.Where(r => r.DueAt != null && r.DueAt < now &&
                r.Status != DataPrincipalRequestStatus.COMPLETED &&
                r.Status != DataPrincipalRequestStatus.REJECTED &&
                r.Status != DataPrincipalRequestStatus.CLOSED);
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        query = query.OrderByDescending(r => r.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<DataPrincipalRequestSummaryDto>(
            items.Select(r => ConsentPrivacyMapper.ToSummaryDto(r, dateTimeProvider)).ToList(), page, pageSize, totalCount);
    }
}

public sealed record GetDataPrincipalRequestByIdQuery(Guid Id) : IRequest<DataPrincipalRequestDetailDto>;

public sealed class GetDataPrincipalRequestByIdQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetDataPrincipalRequestByIdQuery, DataPrincipalRequestDetailDto>
{
    public async Task<DataPrincipalRequestDetailDto> Handle(GetDataPrincipalRequestByIdQuery request, CancellationToken cancellationToken) =>
        ConsentPrivacyMapper.ToDetailDto(await DataPrincipalRequestLoader.LoadForDetailAsync(db, request.Id, cancellationToken), dateTimeProvider);
}

/// <summary>Aggregate counts for an SLA dashboard widget — computed on demand from current DueAt/Status, never persisted, since there is no scheduler in this codebase (see MarkConsentExpiredCommand precedent).</summary>
public sealed record GetDataPrincipalRequestSlaSummaryQuery : IRequest<SlaSummaryDto>;

public sealed class GetDataPrincipalRequestSlaSummaryQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetDataPrincipalRequestSlaSummaryQuery, SlaSummaryDto>
{
    public async Task<SlaSummaryDto> Handle(GetDataPrincipalRequestSlaSummaryQuery request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var within48Hours = now.AddHours(48);

        var open = db.DataPrincipalRequests.AsNoTracking().Where(r =>
            r.Status != DataPrincipalRequestStatus.COMPLETED &&
            r.Status != DataPrincipalRequestStatus.REJECTED &&
            r.Status != DataPrincipalRequestStatus.CLOSED);

        var openCount = await open.CountAsync(cancellationToken);
        var overdueCount = await open.CountAsync(r => r.DueAt != null && r.DueAt < now, cancellationToken);
        var dueWithin48HoursCount = await open.CountAsync(r => r.DueAt != null && r.DueAt >= now && r.DueAt <= within48Hours, cancellationToken);
        var noSlaConfiguredCount = await open.CountAsync(r => r.DueAt == null, cancellationToken);

        return new SlaSummaryDto(openCount, overdueCount, dueWithin48HoursCount, noSlaConfiguredCount);
    }
}
