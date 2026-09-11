using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Evidence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Evidence.Commands;

/// <summary>
/// Batch-transitions APPROVED evidence whose ExpiryDate has passed to
/// EXPIRED, scoped to the caller's organisation. No scheduler exists yet
/// in this codebase (see docs/EVIDENCE_STORAGE.md), so this is exposed as
/// an authenticated endpoint a reviewer can trigger on demand; wiring it
/// to a recurring job later is a drop-in change, not a redesign.
/// </summary>
public sealed record MarkEvidenceExpiredCommand : IRequest<int>;

public sealed class MarkEvidenceExpiredCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<MarkEvidenceExpiredCommand, int>
{
    public async Task<int> Handle(MarkEvidenceExpiredCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);

        var expired = await db.EvidenceItems
            .Where(e => e.Status == EvidenceStatus.APPROVED && e.ExpiryDate != null && e.ExpiryDate < today)
            .ToListAsync(cancellationToken);

        foreach (var evidence in expired)
        {
            evidence.Status = EvidenceStatus.EXPIRED;
            await auditLogger.LogAsync("evidence.expired", nameof(EvidenceItem), evidence.Id.ToString(), cancellationToken: cancellationToken);
        }

        if (expired.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return expired.Count;
    }
}
