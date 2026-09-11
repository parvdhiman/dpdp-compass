using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Findings.DTOs;
using DPDP.Domain.Modules.Assessments;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Findings;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Findings.Commands;

/// <summary>
/// The Module 6 workflow's first three steps: "Assessment -&gt; Failed/Partial
/// Control -&gt; Finding." Only a FAIL or PARTIAL AssessmentControl can
/// generate a finding this way — refuses otherwise. Idempotent: calling
/// this twice for the same control returns a 409, not a duplicate finding.
/// </summary>
public sealed record CreateFindingFromAssessmentControlCommand(Guid AssessmentControlId, string? Recommendation) : IRequest<FindingDetailDto>;

public sealed class CreateFindingFromAssessmentControlCommandValidator : AbstractValidator<CreateFindingFromAssessmentControlCommand>
{
    public CreateFindingFromAssessmentControlCommandValidator()
    {
        RuleFor(x => x.AssessmentControlId).NotEmpty();
        RuleFor(x => x.Recommendation).MaximumLength(2000);
    }
}

public sealed class CreateFindingFromAssessmentControlCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateFindingFromAssessmentControlCommand, FindingDetailDto>
{
    public async Task<FindingDetailDto> Handle(CreateFindingFromAssessmentControlCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create a finding.");
        }

        var assessmentControl = await db.AssessmentControls
            .Include(c => c.Assessment)
            .Include(c => c.Control)
            .FirstOrDefaultAsync(c => c.Id == request.AssessmentControlId, cancellationToken)
            ?? throw new NotFoundException(nameof(AssessmentControl), request.AssessmentControlId);

        if (assessmentControl.Status is not (AnswerStatus.FAIL or AnswerStatus.PARTIAL))
        {
            throw new ConflictException("A finding can only be generated from a control assessed as FAIL or PARTIAL.");
        }

        var alreadyExists = await db.Findings.AnyAsync(f => f.AssessmentControlId == request.AssessmentControlId, cancellationToken);
        if (alreadyExists)
        {
            throw new ConflictException("A finding has already been generated from this control.");
        }

        var control = assessmentControl.Control;
        var severity = control.RiskLevel switch
        {
            RiskLevel.CRITICAL => FindingSeverity.CRITICAL,
            RiskLevel.HIGH => FindingSeverity.HIGH,
            RiskLevel.MEDIUM => FindingSeverity.MEDIUM,
            _ => FindingSeverity.LOW,
        };

        var finding = new Finding
        {
            OrganisationId = organisationId,
            Title = $"{control.ControlId} — {control.Name}",
            Description = $"Assessed as {assessmentControl.Status} during assessment \"{assessmentControl.Assessment.Name}\". {control.Description}",
            Source = FindingSource.ASSESSMENT,
            AssessmentId = assessmentControl.AssessmentId,
            AssessmentControlId = assessmentControl.Id,
            ControlId = control.Id,
            Control = control,
            Severity = severity,
            Status = FindingStatus.OPEN,
            Recommendation = request.Recommendation ?? control.Guidance,
        };

        db.Findings.Add(finding);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "findings.created_from_assessment",
            nameof(Finding),
            finding.Id.ToString(),
            newValue: new { finding.Title, finding.Severity, AssessmentControlId = assessmentControl.Id },
            cancellationToken: cancellationToken);

        return FindingMapper.ToDetailDto(finding);
    }
}
