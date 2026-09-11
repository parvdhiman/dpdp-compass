using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Assessments.DTOs;
using DPDP.Domain.Modules.Assessments;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Assessments.Commands.UpdateAssessment;

public sealed record UpdateAssessmentCommand(Guid Id, string Name, string? Description, DateOnly? DueDate) : IRequest<AssessmentDetailDto>;

public sealed class UpdateAssessmentCommandValidator : AbstractValidator<UpdateAssessmentCommand>
{
    public UpdateAssessmentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class UpdateAssessmentCommandHandler(IAppDbContext db, IAuditLogger auditLogger)
    : IRequestHandler<UpdateAssessmentCommand, AssessmentDetailDto>
{
    public async Task<AssessmentDetailDto> Handle(UpdateAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await LoadForDetailAsync(db, request.Id, cancellationToken);

        if (assessment.Status is not (AssessmentStatus.DRAFT or AssessmentStatus.IN_PROGRESS))
        {
            throw new ConflictException("Only a draft or in-progress assessment can be edited.");
        }

        assessment.Name = request.Name.Trim();
        assessment.Description = request.Description;
        assessment.DueDate = request.DueDate;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("assessments.updated", nameof(Assessment), assessment.Id.ToString(), cancellationToken: cancellationToken);

        return AssessmentMapper.ToDetailDto(assessment);
    }

    internal static async Task<Assessment> LoadForDetailAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.Assessments
            .Include(a => a.FrameworkVersion).ThenInclude(v => v.Framework)
            .Include(a => a.AssignedToUser)
            .Include(a => a.Scopes).ThenInclude(s => s.BusinessUnit)
            .Include(a => a.Scopes).ThenInclude(s => s.Department)
            .Include(a => a.Controls).ThenInclude(c => c.Control).ThenInclude(c => c.ControlCategory)
            .Include(a => a.Controls).ThenInclude(c => c.Questions).ThenInclude(q => q.Question)
            .Include(a => a.Controls).ThenInclude(c => c.Questions).ThenInclude(q => q.Answer)
            .Include(a => a.Reviews).ThenInclude(r => r.Reviewer)
            .Include(a => a.Approvals).ThenInclude(ap => ap.DecidedByUser)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Assessment), id);
}
