using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Domain.Modules.Compliance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Compliance.Commands.DeleteAssessmentQuestion;

public sealed record DeleteAssessmentQuestionCommand(Guid Id) : IRequest;

public sealed class DeleteAssessmentQuestionCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<DeleteAssessmentQuestionCommand>
{
    public async Task Handle(DeleteAssessmentQuestionCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException("Only a Super Administrator can manage the compliance control library.");
        }

        var question = await db.AssessmentQuestions.FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(AssessmentQuestion), request.Id);

        question.IsDeleted = true;
        question.DeletedAt = dateTimeProvider.UtcNow;
        question.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("compliance.question_deleted", nameof(AssessmentQuestion), question.Id.ToString(), cancellationToken: cancellationToken);
    }
}
