using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Findings.DTOs;
using DPDP.Domain.Modules.Findings;
using FluentValidation;
using MediatR;

namespace DPDP.Application.Modules.Findings.Commands;

/// <summary>Formally accepting residual risk instead of remediating — a closing-authority decision, gated the same as CloseFindingCommand.</summary>
public sealed record AcceptFindingRiskCommand(Guid Id, string Comments) : IRequest<FindingDetailDto>;

public sealed class AcceptFindingRiskCommandValidator : AbstractValidator<AcceptFindingRiskCommand>
{
    public AcceptFindingRiskCommandValidator()
    {
        RuleFor(x => x.Comments).NotEmpty().MaximumLength(2000);
    }
}

public sealed class AcceptFindingRiskCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<AcceptFindingRiskCommand, FindingDetailDto>
{
    public async Task<FindingDetailDto> Handle(AcceptFindingRiskCommand request, CancellationToken cancellationToken)
    {
        var finding = await FindingLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        if (!FindingStatusTransitions.CanTransition(finding.Status, FindingStatus.ACCEPTED_RISK))
        {
            throw new ConflictException($"Cannot accept risk for a finding in {finding.Status} status.");
        }

        finding.Status = FindingStatus.ACCEPTED_RISK;
        finding.Recommendation = $"{finding.Recommendation}\n\nRisk accepted: {request.Comments}".Trim();

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("findings.risk_accepted", nameof(Finding), finding.Id.ToString(), newValue: new { request.Comments }, cancellationToken: cancellationToken);

        return FindingMapper.ToDetailDto(finding);
    }
}
