using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.Findings.DTOs;
using DPDP.Domain.Modules.Findings;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DPDP.Application.Modules.Findings.Commands;

public sealed record UpdateFindingCommand(
    Guid Id,
    string Title,
    string Description,
    string Severity,
    string? AssetReference,
    DateOnly? DueDate,
    string? Recommendation) : IRequest<FindingDetailDto>;

public sealed class UpdateFindingCommandValidator : AbstractValidator<UpdateFindingCommand>
{
    public UpdateFindingCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Severity).Must(v => Enum.TryParse<FindingSeverity>(v, out _));
        RuleFor(x => x.Recommendation).MaximumLength(2000);
    }
}

internal static class FindingLoader
{
    public static async Task<Finding> LoadForDetailAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.Findings
            .Include(f => f.Control)
            .Include(f => f.Risk)
            .Include(f => f.Owner)
            .Include(f => f.RemediationTasks).ThenInclude(t => t.Owner)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Finding), id);
}

public sealed class UpdateFindingCommandHandler(IAppDbContext db, IAuditLogger auditLogger) : IRequestHandler<UpdateFindingCommand, FindingDetailDto>
{
    public async Task<FindingDetailDto> Handle(UpdateFindingCommand request, CancellationToken cancellationToken)
    {
        var finding = await FindingLoader.LoadForDetailAsync(db, request.Id, cancellationToken);

        finding.Title = request.Title.Trim();
        finding.Description = request.Description.Trim();
        finding.Severity = Enum.Parse<FindingSeverity>(request.Severity);
        finding.AssetReference = request.AssetReference;
        finding.DueDate = request.DueDate;
        finding.Recommendation = request.Recommendation;

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("findings.updated", nameof(Finding), finding.Id.ToString(), cancellationToken: cancellationToken);

        return FindingMapper.ToDetailDto(finding);
    }
}
