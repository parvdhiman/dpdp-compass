using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Risks.DTOs;
using DPDP.Application.Modules.Risks.Scoring;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Risks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Risks.Commands;

public sealed record UpdateRiskCommand(
    Guid Id,
    string Title,
    string Description,
    string Likelihood,
    string Impact,
    string DataSensitivity,
    string Exposure,
    Guid? OwnerUserId,
    string Status,
    string? TreatmentPlan) : IRequest<RiskDetailDto>;

public sealed class UpdateRiskCommandValidator : AbstractValidator<UpdateRiskCommand>
{
    public UpdateRiskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Likelihood).Must(v => Enum.TryParse<Domain.Modules.Risks.Likelihood>(v, out _));
        RuleFor(x => x.Impact).Must(v => Enum.TryParse<Domain.Modules.Risks.Impact>(v, out _));
        RuleFor(x => x.DataSensitivity).Must(v => Enum.TryParse<RiskLevel>(v, out _));
        RuleFor(x => x.Exposure).Must(v => Enum.TryParse<RiskLevel>(v, out _));
        RuleFor(x => x.Status).Must(v => Enum.TryParse<RiskStatus>(v, out _))
            .WithMessage("status must be one of: " + string.Join(", ", Enum.GetNames<RiskStatus>()));
    }
}

public sealed class UpdateRiskCommandHandler(IAppDbContext db, IRiskScoringStrategy riskScoringStrategy, IAuditLogger auditLogger)
    : IRequestHandler<UpdateRiskCommand, RiskDetailDto>
{
    public async Task<RiskDetailDto> Handle(UpdateRiskCommand request, CancellationToken cancellationToken)
    {
        var risk = await db.Risks
            .Include(r => r.Owner)
            .Include(r => r.Findings)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Risk), request.Id);

        User? owner = risk.Owner;
        if (request.OwnerUserId != risk.OwnerUserId)
        {
            owner = request.OwnerUserId is { } ownerId
                ? await db.Users.FirstOrDefaultAsync(u => u.Id == ownerId, cancellationToken) ?? throw new NotFoundException(nameof(User), ownerId)
                : null;
        }

        var likelihood = Enum.Parse<Domain.Modules.Risks.Likelihood>(request.Likelihood);
        var impact = Enum.Parse<Domain.Modules.Risks.Impact>(request.Impact);
        var dataSensitivity = Enum.Parse<RiskLevel>(request.DataSensitivity);
        var exposure = Enum.Parse<RiskLevel>(request.Exposure);
        var scoreResult = riskScoringStrategy.Calculate(likelihood, impact, dataSensitivity, exposure);

        risk.Title = request.Title.Trim();
        risk.Description = request.Description.Trim();
        risk.Likelihood = likelihood;
        risk.Impact = impact;
        risk.DataSensitivity = dataSensitivity;
        risk.Exposure = exposure;
        risk.CalculatedRiskLevel = scoreResult.Level;
        risk.CalculatedRiskScore = scoreResult.Score;
        risk.OwnerUserId = owner?.Id;
        risk.Owner = owner;
        risk.Status = Enum.Parse<RiskStatus>(request.Status);
        risk.TreatmentPlan = request.TreatmentPlan;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("risks.updated", nameof(Risk), risk.Id.ToString(), newValue: new { risk.CalculatedRiskLevel, risk.Status }, cancellationToken: cancellationToken);

        return RiskMapper.ToDetailDto(risk);
    }
}
