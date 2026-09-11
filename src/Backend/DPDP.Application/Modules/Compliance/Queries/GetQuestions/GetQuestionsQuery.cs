using DPDP.Application.Common.Interfaces;
using DPDP.Application.Common.Models;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Queries.GetQuestions;

public sealed record GetQuestionsQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    Guid? ControlId = null,
    string? QuestionType = null) : IRequest<PagedResult<AssessmentQuestionDto>>;

/// <summary>Same organisation-facing visibility rule as controls: a non-Super-Administrator never sees questions belonging to a non-ACTIVE control.</summary>
public sealed class GetQuestionsQueryHandler(IAppDbContext db, ICurrentUserContext currentUser)
    : IRequestHandler<GetQuestionsQuery, PagedResult<AssessmentQuestionDto>>
{
    public async Task<PagedResult<AssessmentQuestionDto>> Handle(GetQuestionsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.AssessmentQuestions
            .AsNoTracking()
            .Include(q => q.Control)
            .Include(q => q.EvidenceRequirements)
            .AsQueryable();

        if (!currentUser.IsSuperAdministrator)
        {
            query = query.Where(q => q.Control.Status == ControlStatus.ACTIVE);
        }

        if (request.ControlId is { } controlId)
        {
            query = query.Where(q => q.ControlId == controlId);
        }

        if (!string.IsNullOrWhiteSpace(request.QuestionType) && Enum.TryParse<Domain.Modules.Compliance.QuestionType>(request.QuestionType, true, out var type))
        {
            query = query.Where(q => q.QuestionType == type);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToUpperInvariant();
            query = query.Where(q => q.Text.ToUpper().Contains(term) || q.Code.ToUpper().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var questions = await query
            .OrderBy(q => q.Control.ControlId).ThenBy(q => q.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = questions.Select(ComplianceMapper.ToDto).ToList();
        return new PagedResult<AssessmentQuestionDto>(items, page, pageSize, totalCount);
    }
}
