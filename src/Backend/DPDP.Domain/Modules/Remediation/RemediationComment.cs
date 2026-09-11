using DPDP.Domain.Common;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.Remediation;

/// <summary>An append-only discussion thread on a remediation task — never edited or deleted.</summary>
public sealed class RemediationComment : Entity, ITenantScoped
{
    public Guid OrganisationId { get; set; }

    public Guid RemediationTaskId { get; set; }
    public RemediationTask RemediationTask { get; set; } = null!;

    public Guid AuthorUserId { get; set; }
    public User Author { get; set; } = null!;

    public string Comment { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
