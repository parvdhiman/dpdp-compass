using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.ConsentPrivacy.Commands;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using DPDP.Domain.Modules.ConsentPrivacy;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.ConsentPrivacy.Queries;

public sealed record GetConsentRecordsQuery(
    int Page = 1, int PageSize = 25, string? Status = null, Guid? DataPrincipalId = null, Guid? ConsentPurposeId = null)
    : IRequest<PagedResult<ConsentRecordDto>>;

public sealed class GetConsentRecordsQueryHandler(IAppDbContext db) : IRequestHandler<GetConsentRecordsQuery, PagedResult<ConsentRecordDto>>
{
    public async Task<PagedResult<ConsentRecordDto>> Handle(GetConsentRecordsQuery request, CancellationToken cancellationToken)
    {
        var query = db.ConsentRecords
            .AsNoTracking()
            .Include(c => c.DataPrincipal)
            .Include(c => c.ConsentPurpose)
            .Include(c => c.NoticeVersion)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ConsentStatus>(request.Status, true, out var status))
        {
            query = query.Where(c => c.Status == status);
        }

        if (request.DataPrincipalId is { } principalId) query = query.Where(c => c.DataPrincipalId == principalId);
        if (request.ConsentPurposeId is { } purposeId) query = query.Where(c => c.ConsentPurposeId == purposeId);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        query = query.OrderByDescending(c => c.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<ConsentRecordDto>(items.Select(ConsentPrivacyMapper.ToDto).ToList(), page, pageSize, totalCount);
    }
}

public sealed record GetConsentRecordByIdQuery(Guid Id) : IRequest<ConsentRecordDto>;

public sealed class GetConsentRecordByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetConsentRecordByIdQuery, ConsentRecordDto>
{
    public async Task<ConsentRecordDto> Handle(GetConsentRecordByIdQuery request, CancellationToken cancellationToken) =>
        ConsentPrivacyMapper.ToDto(await ConsentRecordLoader.LoadForDetailAsync(db, request.Id, cancellationToken));
}
