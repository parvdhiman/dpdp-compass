using DPDP.Application.Common.Csv;
using DPDP.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.DataInventory.Queries;

/// <summary>Same filters as GetDataInventoryItemsQuery, unpaginated — the CSV is the whole matching set.</summary>
public sealed record ExportDataInventoryQuery(
    string? Search = null, Guid? DataCategoryId = null, Guid? ItSystemId = null, Guid? ProcessorId = null,
    string? Classification = null, string? RiskLevel = null) : IRequest<string>;

public sealed class ExportDataInventoryQueryHandler(IAppDbContext db) : IRequestHandler<ExportDataInventoryQuery, string>
{
    private static readonly string[] Headers =
    [
        "Item Number", "Data Element", "Data Category", "Classification", "Source", "System",
        "Owner", "Purpose", "Retention Policy", "Sharing", "Processor", "Risk Level", "Created At",
    ];

    public async Task<string> Handle(ExportDataInventoryQuery request, CancellationToken cancellationToken)
    {
        var listQuery = GetDataInventoryItemsQueryHandler.BuildFilteredQuery(db, new GetDataInventoryItemsQuery(
            Search: request.Search, DataCategoryId: request.DataCategoryId, ItSystemId: request.ItSystemId,
            ProcessorId: request.ProcessorId, Classification: request.Classification, RiskLevel: request.RiskLevel));

        var items = await listQuery.OrderBy(i => i.DataElementName).ToListAsync(cancellationToken);

        var rows = items.Select(i => (IReadOnlyList<string?>)
        [
            DataInventoryMapper.ItemNumber(i), i.DataElementName, i.DataCategory?.Name, i.Classification?.ToString(),
            i.DataCollectionSource?.Name, i.ItSystem?.Name, i.Owner?.FullName, i.Purpose,
            i.RetentionPolicy?.Name, i.SharingDescription, i.Processor?.Name, i.RiskLevel?.ToString(),
            i.CreatedAt.ToString("O"),
        ]);

        return CsvWriter.Write(Headers, rows);
    }
}
