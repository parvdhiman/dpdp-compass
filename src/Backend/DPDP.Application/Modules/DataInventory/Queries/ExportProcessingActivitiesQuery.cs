using DPDP.Application.Common.Csv;
using DPDP.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.DataInventory.Queries;

/// <summary>The Record of Processing Activities export — same filters as GetProcessingActivitiesQuery, unpaginated.</summary>
public sealed record ExportProcessingActivitiesQuery(string? Search = null, string? Status = null, Guid? OwnerUserId = null) : IRequest<string>;

public sealed class ExportProcessingActivitiesQueryHandler(IAppDbContext db) : IRequestHandler<ExportProcessingActivitiesQuery, string>
{
    private static readonly string[] Headers =
    [
        "Activity Number", "Name", "Purpose", "Status", "Owner", "Data Subject Categories",
        "Data Categories", "Systems", "Data Sources", "Recipients", "Processors",
        "Retention Policy", "Security Controls", "Review Date", "Created At",
    ];

    public async Task<string> Handle(ExportProcessingActivitiesQuery request, CancellationToken cancellationToken)
    {
        var listQuery = GetProcessingActivitiesQueryHandler.BuildFilteredQuery(db, new GetProcessingActivitiesQuery(
            Search: request.Search, Status: request.Status, OwnerUserId: request.OwnerUserId));

        var activities = await listQuery
            .Include(a => a.ItSystems)
            .Include(a => a.DataCollectionSources)
            .Include(a => a.Recipients)
            .Include(a => a.Processors)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        var rows = activities.Select(a => (IReadOnlyList<string?>)
        [
            DataInventoryMapper.ActivityNumber(a), a.Name, a.Purpose, a.Status.ToString(), a.Owner?.FullName,
            string.Join("; ", DataInventoryMapper.ParseJsonArray(a.DataSubjectCategoriesJson)),
            string.Join("; ", a.DataCategories.Select(c => c.Name)),
            string.Join("; ", a.ItSystems.Select(s => s.Name)),
            string.Join("; ", a.DataCollectionSources.Select(s => s.Name)),
            string.Join("; ", a.Recipients.Select(r => r.Name)),
            string.Join("; ", a.Processors.Select(p => p.Name)),
            a.RetentionPolicy?.Name,
            string.Join("; ", DataInventoryMapper.ParseJsonArray(a.SecurityControlsJson)),
            a.ReviewDate?.ToString("yyyy-MM-dd"),
            a.CreatedAt.ToString("O"),
        ]);

        return CsvWriter.Write(Headers, rows);
    }
}
