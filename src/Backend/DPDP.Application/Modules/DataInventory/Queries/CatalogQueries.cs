using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.DataInventory.Commands;
using DPDP.Application.Modules.DataInventory.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.DataInventory.Queries;

/// <summary>
/// List/GetById for the six shared catalogues — grouped in one file for
/// the same reason their DTOs and commands are (see
/// docs/DATA_INVENTORY.md). Each list is small enough in practice that a
/// single page (up to 200) covers typical use without pagination
/// complexity, but is still bounded to prevent an unbounded query.
/// </summary>
public sealed record GetDataCategoriesQuery(string? Search = null, bool? IsActive = null) : IRequest<IReadOnlyList<DataCategoryDto>>;

public sealed class GetDataCategoriesQueryHandler(IAppDbContext db) : IRequestHandler<GetDataCategoriesQuery, IReadOnlyList<DataCategoryDto>>
{
    public async Task<IReadOnlyList<DataCategoryDto>> Handle(GetDataCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = db.DataCategories.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search)) query = query.Where(c => c.Name.ToUpper().Contains(request.Search.Trim().ToUpperInvariant()));
        if (request.IsActive is { } isActive) query = query.Where(c => c.IsActive == isActive);
        var items = await query.OrderBy(c => c.Name).Take(200).ToListAsync(cancellationToken);
        return items.Select(DataInventoryMapper.ToDto).ToList();
    }
}

public sealed record GetDataCategoryByIdQuery(Guid Id) : IRequest<DataCategoryDto>;

public sealed class GetDataCategoryByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetDataCategoryByIdQuery, DataCategoryDto>
{
    public async Task<DataCategoryDto> Handle(GetDataCategoryByIdQuery request, CancellationToken cancellationToken) =>
        DataInventoryMapper.ToDto(await DataCategoryLoader.LoadAsync(db, request.Id, cancellationToken));
}

public sealed record GetItSystemsQuery(string? Search = null, bool? IsActive = null) : IRequest<IReadOnlyList<ItSystemDto>>;

public sealed class GetItSystemsQueryHandler(IAppDbContext db) : IRequestHandler<GetItSystemsQuery, IReadOnlyList<ItSystemDto>>
{
    public async Task<IReadOnlyList<ItSystemDto>> Handle(GetItSystemsQuery request, CancellationToken cancellationToken)
    {
        var query = db.ItSystems.AsNoTracking().Include(s => s.Owner).AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search)) query = query.Where(s => s.Name.ToUpper().Contains(request.Search.Trim().ToUpperInvariant()));
        if (request.IsActive is { } isActive) query = query.Where(s => s.IsActive == isActive);
        var items = await query.OrderBy(s => s.Name).Take(200).ToListAsync(cancellationToken);
        return items.Select(DataInventoryMapper.ToDto).ToList();
    }
}

public sealed record GetItSystemByIdQuery(Guid Id) : IRequest<ItSystemDto>;

public sealed class GetItSystemByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetItSystemByIdQuery, ItSystemDto>
{
    public async Task<ItSystemDto> Handle(GetItSystemByIdQuery request, CancellationToken cancellationToken) =>
        DataInventoryMapper.ToDto(await ItSystemLoader.LoadAsync(db, request.Id, cancellationToken));
}

public sealed record GetDataCollectionSourcesQuery(string? Search = null, bool? IsActive = null) : IRequest<IReadOnlyList<DataCollectionSourceDto>>;

public sealed class GetDataCollectionSourcesQueryHandler(IAppDbContext db) : IRequestHandler<GetDataCollectionSourcesQuery, IReadOnlyList<DataCollectionSourceDto>>
{
    public async Task<IReadOnlyList<DataCollectionSourceDto>> Handle(GetDataCollectionSourcesQuery request, CancellationToken cancellationToken)
    {
        var query = db.DataCollectionSources.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search)) query = query.Where(s => s.Name.ToUpper().Contains(request.Search.Trim().ToUpperInvariant()));
        if (request.IsActive is { } isActive) query = query.Where(s => s.IsActive == isActive);
        var items = await query.OrderBy(s => s.Name).Take(200).ToListAsync(cancellationToken);
        return items.Select(DataInventoryMapper.ToDto).ToList();
    }
}

public sealed record GetDataCollectionSourceByIdQuery(Guid Id) : IRequest<DataCollectionSourceDto>;

public sealed class GetDataCollectionSourceByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetDataCollectionSourceByIdQuery, DataCollectionSourceDto>
{
    public async Task<DataCollectionSourceDto> Handle(GetDataCollectionSourceByIdQuery request, CancellationToken cancellationToken) =>
        DataInventoryMapper.ToDto(await DataCollectionSourceLoader.LoadAsync(db, request.Id, cancellationToken));
}

public sealed record GetProcessorsQuery(string? Search = null, bool? IsActive = null) : IRequest<IReadOnlyList<ProcessorDto>>;

public sealed class GetProcessorsQueryHandler(IAppDbContext db) : IRequestHandler<GetProcessorsQuery, IReadOnlyList<ProcessorDto>>
{
    public async Task<IReadOnlyList<ProcessorDto>> Handle(GetProcessorsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Processors.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search)) query = query.Where(p => p.Name.ToUpper().Contains(request.Search.Trim().ToUpperInvariant()));
        if (request.IsActive is { } isActive) query = query.Where(p => p.IsActive == isActive);
        var items = await query.OrderBy(p => p.Name).Take(200).ToListAsync(cancellationToken);
        return items.Select(DataInventoryMapper.ToDto).ToList();
    }
}

public sealed record GetProcessorByIdQuery(Guid Id) : IRequest<ProcessorDto>;

public sealed class GetProcessorByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetProcessorByIdQuery, ProcessorDto>
{
    public async Task<ProcessorDto> Handle(GetProcessorByIdQuery request, CancellationToken cancellationToken) =>
        DataInventoryMapper.ToDto(await ProcessorLoader.LoadAsync(db, request.Id, cancellationToken));
}

public sealed record GetRecipientsQuery(string? Search = null, bool? IsActive = null) : IRequest<IReadOnlyList<RecipientDto>>;

public sealed class GetRecipientsQueryHandler(IAppDbContext db) : IRequestHandler<GetRecipientsQuery, IReadOnlyList<RecipientDto>>
{
    public async Task<IReadOnlyList<RecipientDto>> Handle(GetRecipientsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Recipients.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search)) query = query.Where(r => r.Name.ToUpper().Contains(request.Search.Trim().ToUpperInvariant()));
        if (request.IsActive is { } isActive) query = query.Where(r => r.IsActive == isActive);
        var items = await query.OrderBy(r => r.Name).Take(200).ToListAsync(cancellationToken);
        return items.Select(DataInventoryMapper.ToDto).ToList();
    }
}

public sealed record GetRecipientByIdQuery(Guid Id) : IRequest<RecipientDto>;

public sealed class GetRecipientByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetRecipientByIdQuery, RecipientDto>
{
    public async Task<RecipientDto> Handle(GetRecipientByIdQuery request, CancellationToken cancellationToken) =>
        DataInventoryMapper.ToDto(await RecipientLoader.LoadAsync(db, request.Id, cancellationToken));
}

public sealed record GetRetentionPoliciesQuery(string? Search = null, bool? IsActive = null) : IRequest<IReadOnlyList<RetentionPolicyDto>>;

public sealed class GetRetentionPoliciesQueryHandler(IAppDbContext db) : IRequestHandler<GetRetentionPoliciesQuery, IReadOnlyList<RetentionPolicyDto>>
{
    public async Task<IReadOnlyList<RetentionPolicyDto>> Handle(GetRetentionPoliciesQuery request, CancellationToken cancellationToken)
    {
        var query = db.RetentionPolicies.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search)) query = query.Where(p => p.Name.ToUpper().Contains(request.Search.Trim().ToUpperInvariant()));
        if (request.IsActive is { } isActive) query = query.Where(p => p.IsActive == isActive);
        var items = await query.OrderBy(p => p.Name).Take(200).ToListAsync(cancellationToken);
        return items.Select(DataInventoryMapper.ToDto).ToList();
    }
}

public sealed record GetRetentionPolicyByIdQuery(Guid Id) : IRequest<RetentionPolicyDto>;

public sealed class GetRetentionPolicyByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetRetentionPolicyByIdQuery, RetentionPolicyDto>
{
    public async Task<RetentionPolicyDto> Handle(GetRetentionPolicyByIdQuery request, CancellationToken cancellationToken) =>
        DataInventoryMapper.ToDto(await RetentionPolicyLoader.LoadAsync(db, request.Id, cancellationToken));
}
