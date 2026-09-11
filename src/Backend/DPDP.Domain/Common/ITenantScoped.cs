namespace DPDP.Domain.Common;

/// <summary>
/// Marks an entity as belonging to exactly one organisation (tenant).
/// EF Core applies a global query filter on this in DpdpDbContext so
/// tenant scoping cannot be forgotten at the query call site —
/// see docs/ARCHITECTURE.md section 4.
/// </summary>
public interface ITenantScoped
{
    Guid OrganisationId { get; }
}
