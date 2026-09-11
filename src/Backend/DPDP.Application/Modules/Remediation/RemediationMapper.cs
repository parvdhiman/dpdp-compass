using System.Text.Json;
using DPDP.Application.Modules.Remediation.DTOs;
using DPDP.Domain.Modules.Remediation;

namespace DPDP.Application.Modules.Remediation;

internal static class RemediationMapper
{
    private static readonly HashSet<RemediationStatus> TerminalStatuses = [RemediationStatus.VERIFIED, RemediationStatus.CLOSED];

    public static List<EvidenceReferenceDto>? DeserializeEvidence(string? json) =>
        json is null ? null : JsonSerializer.Deserialize<List<EvidenceReferenceDto>>(json);

    public static string? SerializeEvidence(IReadOnlyList<EvidenceReferenceDto>? evidence) =>
        evidence is null || evidence.Count == 0 ? null : JsonSerializer.Serialize(evidence);

    public static bool IsOverdue(RemediationTask task, DateOnly today) =>
        task.DueDate is { } dueDate && dueDate < today && !TerminalStatuses.Contains(task.Status);

    public static RemediationTaskSummaryDto ToSummaryDto(RemediationTask task, DateOnly today) => new(
        task.Id, task.FindingId, Findings.FindingMapper.DisplayNumber(task.Finding), task.Title, task.Status.ToString(),
        task.OwnerUserId, task.Owner?.FullName, task.DueDate, IsOverdue(task, today), task.CreatedAt);

    public static RemediationCommentDto ToDto(RemediationComment comment) => new(
        comment.Id, comment.AuthorUserId, comment.Author?.FullName ?? string.Empty, comment.Comment, comment.CreatedAt);

    public static RemediationTaskDetailDto ToDetailDto(RemediationTask task, DateOnly today) => new(
        task.Id, task.FindingId, Findings.FindingMapper.DisplayNumber(task.Finding), task.Finding.Title, task.Title, task.Description,
        task.OwnerUserId, task.Owner?.FullName, task.DueDate, task.Status.ToString(), IsOverdue(task, today),
        DeserializeEvidence(task.EvidenceJson) ?? [], task.VerifiedByUserId, task.VerifiedByUser?.FullName, task.VerifiedAt,
        task.VerificationNotes, task.CreatedAt, task.UpdatedAt,
        task.Comments.OrderBy(c => c.CreatedAt).Select(ToDto).ToList());
}
