using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.DataDiscovery;

/// <summary>
/// A registered connection to a customer-environment system to be
/// scanned for metadata — never a copy of that system's data. Only
/// connection *coordinates* live here; the secret (password/connection
/// key) is encrypted at rest via IConnectionSecretProtector and is never
/// returned by any query DTO — see docs/DATA_DISCOVERY.md section 2.
/// </summary>
public sealed class DataSource : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DataSourceType SourceType { get; set; }

    // Database-engine connectors (POSTGRESQL/MYSQL/SQLSERVER).
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? DatabaseName { get; set; }
    public string? Username { get; set; }
    public string? EncryptedSecret { get; set; }

    /// <summary>
    /// Optional — restricts a POSTGRESQL/SQLSERVER scan to one schema
    /// instead of every schema in the database. Bounds the blast radius on
    /// a large customer database and lets an operator target a scan
    /// deliberately. Null means "scan every non-system schema".
    /// </summary>
    public string? SchemaFilter { get; set; }

    // FILE_SYSTEM connector only.
    public string? RootPath { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastTestedAt { get; set; }
    public bool? LastTestSucceeded { get; set; }
    public string? LastTestError { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<DiscoveryJob> DiscoveryJobs { get; set; } = new List<DiscoveryJob>();
    public ICollection<DataAsset> DataAssets { get; set; } = new List<DataAsset>();
}
