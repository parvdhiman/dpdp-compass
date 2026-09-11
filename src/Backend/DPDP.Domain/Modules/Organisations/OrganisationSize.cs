namespace DPDP.Domain.Modules.Organisations;

/// <summary>Employee-count bands — kept broad and non-numeric so it never needs a data migration when boundaries are debated.</summary>
public enum OrganisationSize
{
    Micro = 0,
    Small = 1,
    Medium = 2,
    Large = 3,
    Enterprise = 4,
}
