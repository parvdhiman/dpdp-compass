using DPDP.Application.Modules.Findings.DTOs;
using DPDP.Domain.Modules.Findings;

namespace DPDP.Application.Modules.Findings;

internal static class FindingMapper
{
    private static readonly HashSet<FindingStatus> TerminalStatuses = [FindingStatus.RESOLVED, FindingStatus.CLOSED, FindingStatus.ACCEPTED_RISK];

    public static string DisplayNumber(Finding finding) => $"FIND-{finding.SequenceNumber:D5}";

    public static bool IsOverdue(Finding finding, DateOnly today) =>
        finding.DueDate is { } dueDate && dueDate < today && !TerminalStatuses.Contains(finding.Status);

    public static FindingSummaryDto ToSummaryDto(Finding finding, DateOnly today) => new(
        finding.Id, DisplayNumber(finding), finding.Title, finding.Severity.ToString(), finding.Status.ToString(),
        finding.Source.ToString(), finding.OwnerUserId, finding.Owner?.FullName, finding.DueDate,
        IsOverdue(finding, today), finding.Risk?.CalculatedRiskLevel.ToString(), finding.CreatedAt);

    public static FindingDetailDto ToDetailDto(Finding finding) => new(
        finding.Id, DisplayNumber(finding), finding.Title, finding.Description, finding.Source.ToString(),
        finding.AssessmentId, finding.AssessmentControlId, finding.ControlId, finding.Control?.ControlId, finding.Control?.Name,
        finding.AssetReference, finding.RiskId, finding.Risk is null ? null : Risks.RiskMapper.DisplayNumber(finding.Risk),
        finding.Risk?.CalculatedRiskLevel.ToString(), finding.Severity.ToString(), finding.OwnerUserId, finding.Owner?.FullName,
        finding.DueDate, finding.Status.ToString(), finding.Recommendation, finding.CreatedAt, finding.UpdatedAt,
        finding.RemediationTasks.Select(t => new FindingRemediationTaskSummaryDto(t.Id, t.Title, t.Status.ToString(), t.OwnerUserId, t.Owner?.FullName, t.DueDate)).ToList());
}
