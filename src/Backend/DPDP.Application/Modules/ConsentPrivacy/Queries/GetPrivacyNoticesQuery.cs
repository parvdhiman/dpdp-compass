using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.ConsentPrivacy.Commands;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Domain.Modules.ConsentPrivacy;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.ConsentPrivacy.Queries;

public sealed record GetPrivacyNoticesQuery(int Page = 1, int PageSize = 25, string? Search = null, string? Status = null, string? Code = null)
    : IRequest<PagedResult<PrivacyNoticeSummaryDto>>;

public sealed class GetPrivacyNoticesQueryHandler(IAppDbContext db) : IRequestHandler<GetPrivacyNoticesQuery, PagedResult<PrivacyNoticeSummaryDto>>
{
    public async Task<PagedResult<PrivacyNoticeSummaryDto>> Handle(GetPrivacyNoticesQuery request, CancellationToken cancellationToken)
    {
        var query = db.PrivacyNotices.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(n => n.Title.ToUpper().Contains(term) || n.Code.ToUpper().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<PrivacyNoticeStatus>(request.Status, true, out var status))
        {
            query = query.Where(n => n.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            query = query.Where(n => n.Code == request.Code);
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        query = query.OrderByDescending(n => n.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<PrivacyNoticeSummaryDto>(items.Select(ConsentPrivacyMapper.ToSummaryDto).ToList(), page, pageSize, totalCount);
    }
}

public sealed record GetPrivacyNoticeByIdQuery(Guid Id) : IRequest<PrivacyNoticeDetailDto>;

public sealed class GetPrivacyNoticeByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetPrivacyNoticeByIdQuery, PrivacyNoticeDetailDto>
{
    public async Task<PrivacyNoticeDetailDto> Handle(GetPrivacyNoticeByIdQuery request, CancellationToken cancellationToken) =>
        ConsentPrivacyMapper.ToDetailDto(await PrivacyNoticeLoader.LoadForDetailAsync(db, request.Id, cancellationToken));
}
