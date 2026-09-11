using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Findings.DTOs;
using DPDP.Application.Modules.Risks.Scoring;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Findings;
using DPDP.Domain.Modules.Risks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Findings.Commands;

/// <summary>
/// The "Finding -&gt; Risk" workflow step: generates a new Risk Register
/// entry from a finding's severity (as a starting-point likelihood/impact
/// pairing the risk owner can refine later) and links the finding to it.
/// A finding can only be linked to one risk at a time — refuses if
/// already linked.
/// </summary>
public sealed record CreateRiskFromFindingCommand(
    Guid FindingId,
    string Likelihood,
    string Impact,
    string? TreatmentPlan) : IRequest<FindingDetailDto>;

public sealed class CreateRiskFromFindingCommandValidator : AbstractValidator<CreateRiskFromFindingCommand>
{
    public CreateRiskFromFindingCommandValidator()
    {
        RuleFor(x => x.Likelihood).Must(v => Enum.TryParse<Domain.Modules.Risks.Likelihood>(v, out _));
        RuleFor(x => x.Impact).Must(v => Enum.TryParse<Domain.Modules.Risks.Impact>(v, out _));
    }
}

public sealed class CreateRiskFromFindingCommandHandler(IAppDbContext db, IRiskScoringStrategy riskScoringStrategy, IAuditLogger auditLogger)
    : IRequestHandler<CreateRiskFromFindingCommand, FindingDetailDto>
{
    public async Task<FindingDetailDto> Handle(CreateRiskFromFindingCommand request, CancellationToken cancellationToken)
    {
        var finding = await FindingLoader.LoadForDetailAsync(db, request.FindingId, cancellationToken);

        if (finding.RiskId is not null)
        {
            throw new ConflictException("This finding is already linked to a risk.");
        }

        var severityAsRiskLevel = finding.Severity switch
        {
            FindingSeverity.CRITICAL => RiskLevel.CRITICAL,
            FindingSeverity.HIGH => RiskLevel.HIGH,
            FindingSeverity.MEDIUM => RiskLevel.MEDIUM,
            _ => RiskLevel.LOW,
        };

        var likelihood = Enum.Parse<Domain.Modules.Risks.Likelihood>(request.Likelihood);
        var impact = Enum.Parse<Domain.Modules.Risks.Impact>(request.Impact);
        var scoreResult = riskScoringStrategy.Calculate(likelihood, impact, severityAsRiskLevel, severityAsRiskLevel);

        var risk = new Risk
        {
            OrganisationId = finding.OrganisationId,
            Title = $"Risk from finding: {finding.Title}",
            Description = finding.Description,
            Likelihood = likelihood,
            Impact = impact,
            DataSensitivity = severityAsRiskLevel,
            Exposure = severityAsRiskLevel,
            CalculatedRiskLevel = scoreResult.Level,
            CalculatedRiskScore = scoreResult.Score,
            OwnerUserId = finding.OwnerUserId,
            Status = RiskStatus.OPEN,
            TreatmentPlan = request.TreatmentPlan,
        };

        db.Risks.Add(risk);
        finding.RiskId = risk.Id;
        finding.Risk = risk;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("findings.risk_created", nameof(Finding), finding.Id.ToString(), newValue: new { RiskId = risk.Id, risk.CalculatedRiskLevel }, cancellationToken: cancellationToken);

        return FindingMapper.ToDetailDto(finding);
    }
}
