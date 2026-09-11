using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Assessments.Commands.SaveAssessmentAnswer;

/// <summary>"Save progress" — upserts one answer in place; never creates a new AssessmentAnswer row (one already exists per question from CreateAssessmentCommand).</summary>
public sealed record SaveAssessmentAnswerCommand(
    Guid AssessmentControlQuestionId,
    string Status,
    string? AnswerValue,
    IReadOnlyList<string>? AnswerValues,
    string? Comment,
    IReadOnlyList<EvidenceReferenceDto>? Evidence,
    string? Confidence,
    string? AssessedRiskLevel,
    string? RemediationNotes) : IRequest<AssessmentAnswerDto>;

public sealed class SaveAssessmentAnswerCommandValidator : AbstractValidator<SaveAssessmentAnswerCommand>
{
    public SaveAssessmentAnswerCommandValidator()
    {
        RuleFor(x => x.AssessmentControlQuestionId).NotEmpty();
        RuleFor(x => x.Status)
            .Must(status => Enum.TryParse<Domain.Modules.Compliance.AnswerStatus>(status, out _))
            .WithMessage("status must be one of: " + string.Join(", ", Enum.GetNames<Domain.Modules.Compliance.AnswerStatus>()));
        RuleFor(x => x.Confidence)
            .Must(c => c is null || Enum.TryParse<ConfidenceLevel>(c, out _))
            .WithMessage("confidence must be one of: " + string.Join(", ", Enum.GetNames<ConfidenceLevel>()));
        RuleFor(x => x.AssessedRiskLevel)
            .Must(r => r is null || Enum.TryParse<Domain.Modules.Compliance.RiskLevel>(r, out _))
            .WithMessage("assessedRiskLevel must be one of: " + string.Join(", ", Enum.GetNames<Domain.Modules.Compliance.RiskLevel>()));
        RuleFor(x => x.Comment).MaximumLength(2000);
        RuleFor(x => x.RemediationNotes).MaximumLength(2000);
    }
}

public sealed class SaveAssessmentAnswerCommandHandler(IAppDbContext db, IAuditLogger auditLogger)
    : IRequestHandler<SaveAssessmentAnswerCommand, AssessmentAnswerDto>
{
    public async Task<AssessmentAnswerDto> Handle(SaveAssessmentAnswerCommand request, CancellationToken cancellationToken)
    {
        var controlQuestion = await db.AssessmentControlQuestions
            .Include(q => q.Question).ThenInclude(cq => cq.EvidenceRequirements)
            .Include(q => q.Answer)
            .Include(q => q.AssessmentControl).ThenInclude(c => c.Assessment)
            .Include(q => q.AssessmentControl).ThenInclude(c => c.Questions).ThenInclude(sibling => sibling.Question)
            .Include(q => q.AssessmentControl).ThenInclude(c => c.Questions).ThenInclude(sibling => sibling.Answer)
            .FirstOrDefaultAsync(q => q.Id == request.AssessmentControlQuestionId, cancellationToken)
            ?? throw new NotFoundException(nameof(AssessmentControlQuestion), request.AssessmentControlQuestionId);

        var assessment = controlQuestion.AssessmentControl.Assessment;
        if (assessment.Status is not (AssessmentStatus.DRAFT or AssessmentStatus.IN_PROGRESS))
        {
            throw new ConflictException("Cannot edit answers once the assessment has been submitted — reopen it first.");
        }

        var newStatus = Enum.Parse<Domain.Modules.Compliance.AnswerStatus>(request.Status);

        var hasMandatoryEvidenceRequirement = controlQuestion.Question.EvidenceRequirements.Any(e => e.IsMandatory);
        var hasEvidenceAttached = request.Evidence is { Count: > 0 };
        if (hasMandatoryEvidenceRequirement
            && newStatus is Domain.Modules.Compliance.AnswerStatus.PASS or Domain.Modules.Compliance.AnswerStatus.PARTIAL
            && !hasEvidenceAttached)
        {
            throw new ConflictException(
                $"This question has mandatory evidence requirements — attach evidence before marking it {newStatus}.");
        }

        var answer = controlQuestion.Answer!;
        answer.Status = newStatus;
        answer.AnswerValue = request.AnswerValue;
        answer.AnswerValuesJson = AssessmentMapper.SerializeAnswerValues(request.AnswerValues);
        answer.Comment = request.Comment;
        answer.EvidenceJson = AssessmentMapper.SerializeEvidence(request.Evidence);
        answer.Confidence = request.Confidence is null ? null : Enum.Parse<ConfidenceLevel>(request.Confidence);
        answer.AssessedRiskLevel = request.AssessedRiskLevel is null ? null : Enum.Parse<Domain.Modules.Compliance.RiskLevel>(request.AssessedRiskLevel);
        answer.RemediationNotes = request.RemediationNotes;

        RecalculateControlStatus(controlQuestion.AssessmentControl);

        if (assessment.Status == AssessmentStatus.DRAFT)
        {
            assessment.Status = AssessmentStatus.IN_PROGRESS;
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "assessments.answer_saved",
            nameof(AssessmentAnswer),
            answer.Id.ToString(),
            newValue: new { Status = newStatus.ToString() },
            cancellationToken: cancellationToken);

        return AssessmentMapper.ToAnswerDto(controlQuestion);
    }

    internal static void RecalculateControlStatus(AssessmentControl control)
    {
        var requiredStatuses = control.Questions
            .Where(q => q.Question.IsRequired)
            .Select(q => q.Answer!.Status)
            .ToList();

        control.Status = ControlStatusCalculator.CalculateControlStatus(requiredStatuses);
    }
}
