using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Queries.GetControlById;

public sealed record GetControlByIdQuery(Guid Id) : IRequest<ControlDetailDto>;

/// <summary>Same visibility rule as GetControlsQuery: a non-Super-Administrator caller never sees a non-ACTIVE control, even by direct id.</summary>
public sealed class GetControlByIdQueryHandler(IAppDbContext db, ICurrentUserContext currentUser)
    : IRequestHandler<GetControlByIdQuery, ControlDetailDto>
{
    public async Task<ControlDetailDto> Handle(GetControlByIdQuery request, CancellationToken cancellationToken)
    {
        var control = await db.Controls
            .AsNoTracking()
            .Include(c => c.ControlCategory)
            .Include(c => c.ControlMappings).ThenInclude(m => m.Requirement).ThenInclude(r => r.LegalReference)
            .Include(c => c.Questions).ThenInclude(q => q.EvidenceRequirements)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Control), request.Id);

        if (!currentUser.IsSuperAdministrator && control.Status != ControlStatus.ACTIVE)
        {
            throw new NotFoundException(nameof(Control), request.Id);
        }

        return ComplianceMapper.ToDetailDto(control);
    }
}
