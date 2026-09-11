using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Queries.GetControlCategories;

public sealed record GetControlCategoriesQuery : IRequest<IReadOnlyList<ControlCategoryDto>>;

public sealed class GetControlCategoriesQueryHandler(IAppDbContext db)
    : IRequestHandler<GetControlCategoriesQuery, IReadOnlyList<ControlCategoryDto>>
{
    public async Task<IReadOnlyList<ControlCategoryDto>> Handle(GetControlCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await db.ControlCategories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .Select(c => new { Category = c, ControlCount = db.Controls.Count(ctrl => ctrl.ControlCategoryId == c.Id) })
            .ToListAsync(cancellationToken);

        return categories.Select(x => ComplianceMapper.ToDto(x.Category, x.ControlCount)).ToList();
    }
}
