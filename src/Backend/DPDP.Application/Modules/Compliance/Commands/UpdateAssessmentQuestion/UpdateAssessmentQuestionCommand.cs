using System.Text.Json;
using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Compliance.DTOs;
using DPDP.Domain.Modules.Compliance;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.UpdateAssessmentQuestion;

public sealed record UpdateAssessmentQuestionCommand(
    Guid Id,
    string Text,
    string? HelpText,
    string QuestionType,
    IReadOnlyList<string>? Options,
    bool IsRequired,
    int SortOrder) : IRequest<AssessmentQuestionDto>;

public sealed class UpdateAssessmentQuestionCommandValidator : AbstractValidator<UpdateAssessmentQuestionCommand>
{
    public UpdateAssessmentQuestionCommandValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.QuestionType)
            .Must(type => Enum.TryParse<Domain.Modules.Compliance.QuestionType>(type, out _))
            .WithMessage("questionType must be one of: " + string.Join(", ", Enum.GetNames<Domain.Modules.Compliance.QuestionType>()));
    }
}

public sealed class UpdateAssessmentQuestionCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IAuditLogger auditLogger)
    : IRequestHandler<UpdateAssessmentQuestionCommand, AssessmentQuestionDto>
{
    public async Task<AssessmentQuestionDto> Handle(UpdateAssessmentQuestionCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance control library.");
        }

        var question = await db.AssessmentQuestions
            .Include(q => q.Control)
            .Include(q => q.EvidenceRequirements)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(AssessmentQuestion), request.Id);

        question.Text = request.Text.Trim();
        question.HelpText = request.HelpText;
        question.QuestionType = Enum.Parse<Domain.Modules.Compliance.QuestionType>(request.QuestionType);
        question.OptionsJson = request.Options is null ? null : JsonSerializer.Serialize(request.Options);
        question.IsRequired = request.IsRequired;
        question.SortOrder = request.SortOrder;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.question_updated", nameof(AssessmentQuestion), question.Id.ToString(), cancellationToken: cancellationToken);

        return ComplianceMapper.ToDto(question);
    }
}
