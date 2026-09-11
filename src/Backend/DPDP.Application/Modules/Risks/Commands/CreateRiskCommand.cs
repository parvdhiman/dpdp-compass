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

public sealed record CreateRiskCommand(
    string Title,
    string Description,
    string Likelihood,
    string Impact,
    string DataSensitivity,
    string Exposure,
    Guid? OwnerUserId,
    string? TreatmentPlan) : IRequest<RiskDetailDto>;

public sealed class CreateRiskCommandValidator : AbstractValidator<CreateRiskCommand>
{
    public CreateRiskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Likelihood).Must(v => Enum.TryParse<Domain.Modules.Risks.Likelihood>(v, out _))
            .WithMessage("likelihood must be one of: " + string.Join(", ", Enum.GetNames<Domain.Modules.Risks.Likelihood>()));
        RuleFor(x => x.Impact).Must(v => Enum.TryParse<Domain.Modules.Risks.Impact>(v, out _))
            .WithMessage("impact must be one of: " + string.Join(", ", Enum.GetNames<Domain.Modules.Risks.Impact>()));
        RuleFor(x => x.DataSensitivity).Must(v => Enum.TryParse<RiskLevel>(v, out _))
            .WithMessage("dataSensitivity must be one of: " + string.Join(", ", Enum.GetNames<RiskLevel>()));
        RuleFor(x => x.Exposure).Must(v => Enum.TryParse<RiskLevel>(v, out _))
            .WithMessage("exposure must be one of: " + string.Join(", ", Enum.GetNames<RiskLevel>()));
    }
}

public sealed class CreateRiskCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IRiskScoringStrategy riskScoringStrategy, IAuditLogger auditLogger)
    : IRequestHandler<CreateRiskCommand, RiskDetailDto>
{
    public async Task<RiskDetailDto> Handle(CreateRiskCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a risk.");
        }

        User? owner = null;
        if (request.OwnerUserId is { } ownerId)
        {
            owner = await db.Users.FirstOrDefaultAsync(u => u.Id == ownerId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), ownerId);
        }

        var likelihood = Enum.Parse<Domain.Modules.Risks.Likelihood>(request.Likelihood);
        var impact = Enum.Parse<Domain.Modules.Risks.Impact>(request.Impact);
        var dataSensitivity = Enum.Parse<RiskLevel>(request.DataSensitivity);
        var exposure = Enum.Parse<RiskLevel>(request.Exposure);
        var scoreResult = riskScoringStrategy.Calculate(likelihood, impact, dataSensitivity, exposure);

        var risk = new Risk
        {
            OrganisationId = organisationId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Likelihood = likelihood,
            Impact = impact,
            DataSensitivity = dataSensitivity,
            Exposure = exposure,
            CalculatedRiskLevel = scoreResult.Level,
            CalculatedRiskScore = scoreResult.Score,
            OwnerUserId = owner?.Id,
            Owner = owner,
            Status = RiskStatus.OPEN,
            TreatmentPlan = request.TreatmentPlan,
        };

        db.Risks.Add(risk);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("risks.created", nameof(Risk), risk.Id.ToString(), newValue: new { risk.Title, risk.CalculatedRiskLevel }, cancellationToken: cancellationToken);

        return RiskMapper.ToDetailDto(risk);
    }
}
