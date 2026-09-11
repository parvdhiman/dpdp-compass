using DPDP.Domain.Common;

namespace DPDP.Domain.Modules.Organisations;

/// <summary>One of an organisation's physical locations/offices — part of the organisation profile, not a standalone module.</summary>
public sealed class OrganisationLocation : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid OrganisationId { get; set; }
    public Organisation Organisation { get; set; } = null!;

    public string Label { get; set; } = string.Empty;
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsPrimary { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
