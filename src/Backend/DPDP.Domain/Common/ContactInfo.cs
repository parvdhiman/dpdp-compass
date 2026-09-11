namespace DPDP.Domain.Common;

/// <summary>
/// A reusable owned value object for the various named contacts an
/// organisation/business unit/department carries (primary contact,
/// privacy contact, DPO, head of unit/department) — see
/// docs/DATABASE.md section 6. Mapped via EF Core owned-type
/// configuration (OwnsOne), never its own table.
/// </summary>
public sealed class ContactInfo
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
