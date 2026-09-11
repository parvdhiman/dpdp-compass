using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.ConsentPrivacy.Commands;
using DPDP.Application.Modules.ConsentPrivacy.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.ConsentPrivacy.Queries;

/// <summary>List/GetById for the three simple catalogues (DataPrincipal, ConsentPurpose, SlaPolicy) — grouped in one file for the same reason as Module 9's CatalogQueries.cs.</summary>
public sealed record GetDataPrincipalsQuery(string? Search = null, bool? IsActive = null) : IRequest<IReadOnlyList<DataPrincipalDto>>;

public sealed class GetDataPrincipalsQueryHandler(IAppDbContext db) : IRequestHandler<GetDataPrincipalsQuery, IReadOnlyList<DataPrincipalDto>>
{
    public async Task<IReadOnlyList<DataPrincipalDto>> Handle(GetDataPrincipalsQuery request, CancellationToken cancellationToken)
    {
        var query = db.DataPrincipals.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search)) query = query.Where(p => p.ExternalReferenceId.ToUpper().Contains(request.Search.Trim().ToUpperInvariant()));
        if (request.IsActive is { } isActive) query = query.Where(p => p.IsActive == isActive);
        var items = await query.OrderByDescending(p => p.CreatedAt).Take(200).ToListAsync(cancellationToken);
        return items.Select(ConsentPrivacyMapper.ToDto).ToList();
    }
}

public sealed record GetDataPrincipalByIdQuery(Guid Id) : IRequest<DataPrincipalDto>;

public sealed class GetDataPrincipalByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetDataPrincipalByIdQuery, DataPrincipalDto>
{
    public async Task<DataPrincipalDto> Handle(GetDataPrincipalByIdQuery request, CancellationToken cancellationToken) =>
        ConsentPrivacyMapper.ToDto(await DataPrincipalLoader.LoadAsync(db, request.Id, cancellationToken));
}

public sealed record GetConsentPurposesQuery(string? Search = null, bool? IsActive = null) : IRequest<IReadOnlyList<ConsentPurposeDto>>;

public sealed class GetConsentPurposesQueryHandler(IAppDbContext db) : IRequestHandler<GetConsentPurposesQuery, IReadOnlyList<ConsentPurposeDto>>
{
    public async Task<IReadOnlyList<ConsentPurposeDto>> Handle(GetConsentPurposesQuery request, CancellationToken cancellationToken)
    {
        var query = db.ConsentPurposes.AsNoTracking().Include(p => p.DataCategory).AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search)) query = query.Where(p => p.Name.ToUpper().Contains(request.Search.Trim().ToUpperInvariant()));
        if (request.IsActive is { } isActive) query = query.Where(p => p.IsActive == isActive);
        var items = await query.OrderBy(p => p.Name).Take(200).ToListAsync(cancellationToken);
        return items.Select(ConsentPrivacyMapper.ToDto).ToList();
    }
}

public sealed record GetConsentPurposeByIdQuery(Guid Id) : IRequest<ConsentPurposeDto>;

public sealed class GetConsentPurposeByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetConsentPurposeByIdQuery, ConsentPurposeDto>
{
    public async Task<ConsentPurposeDto> Handle(GetConsentPurposeByIdQuery request, CancellationToken cancellationToken) =>
        ConsentPrivacyMapper.ToDto(await ConsentPurposeLoader.LoadAsync(db, request.Id, cancellationToken));
}

public sealed record GetSlaPoliciesQuery(string? Search = null, bool? IsActive = null) : IRequest<IReadOnlyList<SlaPolicyDto>>;

public sealed class GetSlaPoliciesQueryHandler(IAppDbContext db) : IRequestHandler<GetSlaPoliciesQuery, IReadOnlyList<SlaPolicyDto>>
{
    public async Task<IReadOnlyList<SlaPolicyDto>> Handle(GetSlaPoliciesQuery request, CancellationToken cancellationToken)
    {
        var query = db.SlaPolicies.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search)) query = query.Where(p => p.Name.ToUpper().Contains(request.Search.Trim().ToUpperInvariant()));
        if (request.IsActive is { } isActive) query = query.Where(p => p.IsActive == isActive);
        var items = await query.OrderBy(p => p.Name).Take(200).ToListAsync(cancellationToken);
        return items.Select(ConsentPrivacyMapper.ToDto).ToList();
    }
}

public sealed record GetSlaPolicyByIdQuery(Guid Id) : IRequest<SlaPolicyDto>;

public sealed class GetSlaPolicyByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetSlaPolicyByIdQuery, SlaPolicyDto>
{
    public async Task<SlaPolicyDto> Handle(GetSlaPolicyByIdQuery request, CancellationToken cancellationToken) =>
        ConsentPrivacyMapper.ToDto(await SlaPolicyLoader.LoadAsync(db, request.Id, cancellationToken));
}
