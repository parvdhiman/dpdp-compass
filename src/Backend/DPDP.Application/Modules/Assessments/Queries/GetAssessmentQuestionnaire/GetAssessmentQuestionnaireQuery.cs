using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Assessments.Queries.GetAssessmentQuestionnaire;

/// <summary>Backs the Questionnaire/Wizard page — every control and question in one nested structure. Not paginated: an assessment's control/question count is bounded by the control library's own size (see docs/ARCHITECTURE.md Module 5 section).</summary>
public sealed record GetAssessmentQuestionnaireQuery(Guid AssessmentId) : IRequest<IReadOnlyList<AssessmentControlQuestionnaireDto>>;

public sealed class GetAssessmentQuestionnaireQueryHandler(IAppDbContext db)
    : IRequestHandler<GetAssessmentQuestionnaireQuery, IReadOnlyList<AssessmentControlQuestionnaireDto>>
{
    public async Task<IReadOnlyList<AssessmentControlQuestionnaireDto>> Handle(GetAssessmentQuestionnaireQuery request, CancellationToken cancellationToken)
    {
        var assessmentExists = await db.Assessments.AnyAsync(a => a.Id == request.AssessmentId, cancellationToken);
        if (!assessmentExists)
        {
            throw new NotFoundException(nameof(Assessment), request.AssessmentId);
        }

        var controls = await db.AssessmentControls
            .AsNoTracking()
            .Where(c => c.AssessmentId == request.AssessmentId)
            .Include(c => c.Control).ThenInclude(c => c.ControlCategory)
            .Include(c => c.Questions).ThenInclude(q => q.Question).ThenInclude(q => q.EvidenceRequirements)
            .Include(c => c.Questions).ThenInclude(q => q.Answer)
            .Include(c => c.Questions).ThenInclude(q => q.Answer!.Reviewer)
            .OrderBy(c => c.Control.ControlId)
            .ToListAsync(cancellationToken);

        return controls.Select(AssessmentMapper.ToQuestionnaireDto).ToList();
    }
}
