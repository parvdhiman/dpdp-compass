using DPDP.Domain.Modules.Compliance;
using MediatR;

namespace DPDP.Application.Modules.Compliance.Queries.GetAnswerStatuses;

/// <summary>
/// Reference data for the future Assessment Engine module: the fixed set of
/// answer statuses an assessment response can hold. No database access —
/// this enum is defined now (per MASTER_PROMPT Module 4 scope) but not yet
/// used by any persisted entity.
/// </summary>
public sealed record GetAnswerStatusesQuery : IRequest<IReadOnlyList<string>>;

public sealed class GetAnswerStatusesQueryHandler : IRequestHandler<GetAnswerStatusesQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(GetAnswerStatusesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<string> statuses = Enum.GetNames<AnswerStatus>();
        return Task.FromResult(statuses);
    }
}
