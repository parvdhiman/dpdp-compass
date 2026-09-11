using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Remediation.DTOs;
using DPDP.Domain.Modules.Identity;
using DPDP.Domain.Modules.Remediation;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Remediation.Commands;

public sealed record AddRemediationCommentCommand(Guid Id, string Comment) : IRequest<RemediationCommentDto>;

public sealed class AddRemediationCommentCommandValidator : AbstractValidator<AddRemediationCommentCommand>
{
    public AddRemediationCommentCommandValidator()
    {
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(2000);
    }
}

public sealed class AddRemediationCommentCommandHandler(IAppDbContext db, ICurrentUserContext currentUser, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<AddRemediationCommentCommand, RemediationCommentDto>
{
    public async Task<RemediationCommentDto> Handle(AddRemediationCommentCommand request, CancellationToken cancellationToken)
    {
        var task = await db.RemediationTasks.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(RemediationTask), request.Id);

        var author = await db.Users.FirstAsync(u => u.Id == currentUser.UserId!.Value, cancellationToken);

        var comment = new RemediationComment
        {
            OrganisationId = task.OrganisationId,
            RemediationTaskId = request.Id,
            AuthorUserId = author.Id,
            Author = author,
            Comment = request.Comment.Trim(),
            CreatedAt = dateTimeProvider.UtcNow,
        };

        db.RemediationComments.Add(comment);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("remediation.comment_added", nameof(RemediationTask), request.Id.ToString(), cancellationToken: cancellationToken);

        return RemediationMapper.ToDto(comment);
    }
}
