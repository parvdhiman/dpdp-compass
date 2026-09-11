using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using DPDP.Domain.Modules.Compliance;
using DPDP.Domain.Modules.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Assessments.Commands.CreateAssessment;

public sealed record AssessmentScopeInput(Guid? BusinessUnitId, Guid? DepartmentId, string? Notes);

/// <summary>
/// "Create Assessment -&gt; Select Framework Version -&gt; Determine Applicable
/// Controls" (Module 5 workflow) happen together here: every ACTIVE
/// control mapped to a requirement under the chosen FrameworkVersion is
/// snapshotted into this assessment's own AssessmentControl/
/// AssessmentControlQuestion/AssessmentAnswer rows, stable from this point
/// on even if the control library changes later. There is no automated
/// applicability rule engine (see docs/ARCHITECTURE.md Module 5 section) —
/// an assessor marks a specific control NOT_APPLICABLE during the
/// questionnaire instead.
/// </summary>
public sealed record CreateAssessmentCommand(
    Guid FrameworkVersionId,
    string Name,
    string? Description,
    Guid? AssignedToUserId,
    DateOnly? DueDate,
    IReadOnlyList<AssessmentScopeInput>? Scopes) : IRequest<AssessmentDetailDto>;

public sealed class CreateAssessmentCommandValidator : AbstractValidator<CreateAssessmentCommand>
{
    public CreateAssessmentCommandValidator()
    {
        RuleFor(x => x.FrameworkVersionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class CreateAssessmentCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<CreateAssessmentCommand, AssessmentDetailDto>
{
    public async Task<AssessmentDetailDto> Handle(CreateAssessmentCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can create an assessment.");
        }

        var frameworkVersion = await db.FrameworkVersions
            .Include(v => v.Framework)
            .FirstOrDefaultAsync(v => v.Id == request.FrameworkVersionId, cancellationToken)
            ?? throw new NotFoundException(nameof(FrameworkVersion), request.FrameworkVersionId);

        User? assignedUser = null;
        if (request.AssignedToUserId is { } assignedToUserId)
        {
            assignedUser = await db.Users.FirstOrDefaultAsync(u => u.Id == assignedToUserId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), assignedToUserId);
        }

        var now = dateTimeProvider.UtcNow;

        var assessment = new Assessment
        {
            OrganisationId = organisationId,
            FrameworkVersionId = frameworkVersion.Id,
            FrameworkVersion = frameworkVersion,
            Name = request.Name.Trim(),
            Description = request.Description,
            Status = AssessmentStatus.DRAFT,
            AssignedToUserId = assignedUser?.Id,
            AssignedToUser = assignedUser,
            DueDate = request.DueDate,
        };

        if (request.Scopes is { Count: > 0 })
        {
            foreach (var scopeInput in request.Scopes)
            {
                if (scopeInput.BusinessUnitId is { } buId && !await db.BusinessUnits.AnyAsync(b => b.Id == buId, cancellationToken))
                {
                    throw new NotFoundException("BusinessUnit", buId);
                }

                if (scopeInput.DepartmentId is { } deptId && !await db.Departments.AnyAsync(d => d.Id == deptId, cancellationToken))
                {
                    throw new NotFoundException("Department", deptId);
                }

                assessment.Scopes.Add(new AssessmentScope
                {
                    OrganisationId = organisationId,
                    BusinessUnitId = scopeInput.BusinessUnitId,
                    DepartmentId = scopeInput.DepartmentId,
                    Notes = scopeInput.Notes,
                    CreatedAt = now,
                });
            }
        }

        var applicableControls = await db.Controls
            .Where(c => c.Status == ControlStatus.ACTIVE)
            .Where(c => c.ControlMappings.Any(m => m.Requirement.LegalReference.FrameworkVersionId == request.FrameworkVersionId))
            .Include(c => c.ControlCategory)
            .Include(c => c.Questions).ThenInclude(q => q.EvidenceRequirements)
            .ToListAsync(cancellationToken);

        foreach (var control in applicableControls)
        {
            var assessmentControl = new AssessmentControl
            {
                OrganisationId = organisationId,
                Control = control,
                ControlId = control.Id,
                Status = AnswerStatus.NOT_ASSESSED,
                CreatedAt = now,
            };

            foreach (var question in control.Questions)
            {
                var controlQuestion = new AssessmentControlQuestion
                {
                    OrganisationId = organisationId,
                    Question = question,
                    QuestionId = question.Id,
                    CreatedAt = now,
                };

                controlQuestion.Answer = new AssessmentAnswer
                {
                    OrganisationId = organisationId,
                    AssessmentControlQuestion = controlQuestion,
                    Status = AnswerStatus.NOT_ASSESSED,
                };

                assessmentControl.Questions.Add(controlQuestion);
            }

            assessment.Controls.Add(assessmentControl);
        }

        db.Assessments.Add(assessment);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "assessments.created",
            nameof(Assessment),
            assessment.Id.ToString(),
            newValue: new { assessment.Name, FrameworkVersion = frameworkVersion.VersionLabel, ControlCount = applicableControls.Count },
            cancellationToken: cancellationToken);

        return AssessmentMapper.ToDetailDto(assessment);
    }
}
