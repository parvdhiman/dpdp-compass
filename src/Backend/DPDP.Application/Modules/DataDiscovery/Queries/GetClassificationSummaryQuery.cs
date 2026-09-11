using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.DataDiscovery.Queries;

public sealed record GetClassificationSummaryQuery : IRequest<ClassificationSummaryDto>;

public sealed class GetClassificationSummaryQueryHandler(IAppDbContext db) : IRequestHandler<GetClassificationSummaryQuery, ClassificationSummaryDto>
{
    public async Task<ClassificationSummaryDto> Handle(GetClassificationSummaryQuery request, CancellationToken cancellationToken)
    {
        var elements = await db.DataElements.AsNoTracking().ToListAsync(cancellationToken);

        var categoryCounts = elements
            .Where(e => e.ClassificationCategory != null)
            .GroupBy(e => e.ClassificationCategory!.Value)
            .Select(g => new ClassificationCategoryCountDto(g.Key.ToString(), g.Count()))
            .OrderByDescending(c => c.Count)
            .ToList();

        return new ClassificationSummaryDto(
            elements.Count,
            elements.Count(e => e.ClassificationCategory == null),
            elements.Count(e => e.IsHumanCorrected),
            categoryCounts);
    }
}
