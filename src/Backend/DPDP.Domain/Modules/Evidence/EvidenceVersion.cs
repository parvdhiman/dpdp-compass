using DPDP.Domain.Common;
using DPDP.Domain.Modules.Identity;

namespace DPDP.Domain.Modules.Evidence;

/// <summary>
/// One uploaded file (or, for EvidenceType.URL, one external link) —
/// immutable once created. StorageKey is a server-generated path into
/// object storage (see IObjectStorageService), never derived from the
/// client-supplied file name, which is kept only as OriginalFileName
/// metadata — path-traversal prevention per docs/SECURITY.md section 5.
/// </summary>
public sealed class EvidenceVersion : Entity, ITenantScoped
{
    public Guid OrganisationId { get; set; }

    public Guid EvidenceItemId { get; set; }
    public EvidenceItem EvidenceItem { get; set; } = null!;

    public int VersionNumber { get; set; }

    /// <summary>Null only for EvidenceType.URL versions.</summary>
    public string? StorageKey { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long? SizeBytes { get; set; }
    public string? ChecksumSha256 { get; set; }

    /// <summary>Set only for EvidenceType.URL versions.</summary>
    public string? ExternalUrl { get; set; }

    public MalwareScanStatus MalwareScanStatus { get; set; } = MalwareScanStatus.PENDING;
    public string? MalwareScanDetails { get; set; }

    public string? MetadataJson { get; set; }

    public Guid UploadedByUserId { get; set; }
    public User UploadedByUser { get; set; } = null!;
    public DateTimeOffset UploadedAt { get; set; }
}
