using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Assessments.Queries.GetAssessmentById;

public sealed record GetAssessmentByIdQuery(Guid Id) : IRequest<AssessmentDetailDto>;

public sealed class GetAssessmentByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetAssessmentByIdQuery, AssessmentDetailDto>
{
    public async Task<AssessmentDetailDto> Handle(GetAssessmentByIdQuery request, CancellationToken cancellationToken)
    {
        var assessment = await db.Assessments
            .AsNoTracking()
            .Include(a => a.FrameworkVersion).ThenInclude(v => v.Framework)
            .Include(a => a.AssignedToUser)
            .Include(a => a.Scopes).ThenInclude(s => s.BusinessUnit)
            .Include(a => a.Scopes).ThenInclude(s => s.Department)
            .Include(a => a.Controls).ThenInclude(c => c.Control).ThenInclude(c => c.ControlCategory)
            .Include(a => a.Controls).ThenInclude(c => c.Questions).ThenInclude(q => q.Answer)
            .Include(a => a.Reviews).ThenInclude(r => r.Reviewer)
            .Include(a => a.Approvals).ThenInclude(ap => ap.DecidedByUser)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Assessment), request.Id);

        return AssessmentMapper.ToDetailDto(assessment);
    }
}
