using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Remediation.DTOs;
using DPDP.Domain.Modules.Remediation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Remediation.Queries;

public sealed record GetRemediationTaskByIdQuery(Guid Id) : IRequest<RemediationTaskDetailDto>;

public sealed class GetRemediationTaskByIdQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetRemediationTaskByIdQuery, RemediationTaskDetailDto>
{
    public async Task<RemediationTaskDetailDto> Handle(GetRemediationTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await db.RemediationTasks
            .AsNoTracking()
            .Include(t => t.Finding)
            .Include(t => t.Owner)
            .Include(t => t.VerifiedByUser)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(RemediationTask), request.Id);

        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);
        return RemediationMapper.ToDetailDto(task, today);
    }
}
