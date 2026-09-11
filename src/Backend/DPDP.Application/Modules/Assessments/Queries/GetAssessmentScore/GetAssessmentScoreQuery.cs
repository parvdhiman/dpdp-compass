using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Application.Modules.Assessments.Scoring;
using DPDP.Domain.Modules.Assessments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Assessments.Queries.GetAssessmentScore;

/// <summary>
/// "Calculate Score" — always computed live from the assessment's current
/// answers via IComplianceScoringStrategy, never cached/stored on the
/// Assessment row, so it's never possible for a displayed score to go
/// stale relative to the underlying answers.
/// </summary>
public sealed record GetAssessmentScoreQuery(Guid AssessmentId) : IRequest<AssessmentScoreDto>;

public sealed class GetAssessmentScoreQueryHandler(IAppDbContext db, IComplianceScoringStrategy scoringStrategy)
    : IRequestHandler<GetAssessmentScoreQuery, AssessmentScoreDto>
{
    public async Task<AssessmentScoreDto> Handle(GetAssessmentScoreQuery request, CancellationToken cancellationToken)
    {
        var assessmentExists = await db.Assessments.AnyAsync(a => a.Id == request.AssessmentId, cancellationToken);
        if (!assessmentExists)
        {
            throw new NotFoundException(nameof(Assessment), request.AssessmentId);
        }

        var controls = await db.AssessmentControls
            .AsNoTracking()
            .Where(c => c.AssessmentId == request.AssessmentId)
            .Include(c => c.Control)
            .Include(c => c.Questions).ThenInclude(q => q.Question).ThenInclude(q => q.EvidenceRequirements)
            .Include(c => c.Questions).ThenInclude(q => q.Answer)
            .ToListAsync(cancellationToken);

        var scoringInputs = controls.Select(AssessmentMapper.ToScoringInput).ToList();
        var result = scoringStrategy.Calculate(scoringInputs);

        return AssessmentMapper.ToScoreDto(result, controls);
    }
}
