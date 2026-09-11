using System.Text.Json;
using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.CreateAssessmentQuestion;

public sealed record CreateAssessmentQuestionCommand(
    Guid ControlId,
    string Code,
    string Text,
    string? HelpText,
    string QuestionType,
    IReadOnlyList<string>? Options,
    bool IsRequired,
    int SortOrder) : IRequest<AssessmentQuestionDto>;

public sealed class CreateAssessmentQuestionCommandValidator : AbstractValidator<CreateAssessmentQuestionCommand>
{
    private static readonly string[] ChoiceTypes = ["MULTIPLE_CHOICE", "MULTI_SELECT"];

    public CreateAssessmentQuestionCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.QuestionType)
            .Must(type => Enum.TryParse<Domain.Modules.Compliance.QuestionType>(type, out _))
            .WithMessage("questionType must be one of: " + string.Join(", ", Enum.GetNames<Domain.Modules.Compliance.QuestionType>()));
        RuleFor(x => x.Options)
            .NotEmpty()
            .When(x => ChoiceTypes.Contains(x.QuestionType))
            .WithMessage("options is required for MULTIPLE_CHOICE and MULTI_SELECT questions.");
    }
}

public sealed class CreateAssessmentQuestionCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<CreateAssessmentQuestionCommand, AssessmentQuestionDto>
{
    public async Task<AssessmentQuestionDto> Handle(CreateAssessmentQuestionCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance control library.");
        }

        var control = await db.Controls.FirstOrDefaultAsync(c => c.Id == request.ControlId, cancellationToken)
            ?? throw new NotFoundException(nameof(Control), request.ControlId);

        var codeTaken = await db.AssessmentQuestions.AnyAsync(q => q.Code == request.Code, cancellationToken);
        if (codeTaken)
        {
            throw new ConflictException("A question with this code already exists.");
        }

        var question = new AssessmentQuestion
        {
            ControlId = request.ControlId,
            Code = request.Code.Trim(),
            Text = request.Text.Trim(),
            HelpText = request.HelpText,
            QuestionType = Enum.Parse<Domain.Modules.Compliance.QuestionType>(request.QuestionType),
            OptionsJson = request.Options is null ? null : JsonSerializer.Serialize(request.Options),
            IsRequired = request.IsRequired,
            SortOrder = request.SortOrder,
        };

        db.AssessmentQuestions.Add(question);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.question_created", nameof(AssessmentQuestion), question.Id.ToString(), newValue: new { question.Code }, cancellationToken: cancellationToken);

        question.Control = control;
        return ComplianceMapper.ToDto(question);
    }
}
