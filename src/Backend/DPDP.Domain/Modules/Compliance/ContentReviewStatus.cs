namespace DPDP.Domain.Modules.Compliance;

/// <summary>
/// Legal-content governance gate — see docs/COMPLIANCE_CONTENT_GOVERNANCE.md.
/// Never hidden from any API response or UI view: an organisation using
/// DRAFT content must always be able to see that it hasn't been legally
/// reviewed yet. This is orthogonal to ControlStatus — a control can be
/// ACTIVE (operationally usable) and DRAFT (not yet legally reviewed) at
/// the same time; that combination is the expected default for freshly
/// seeded content.
/// </summary>
public enum ContentReviewStatus
{
    DRAFT,
    LEGAL_REVIEWED,
    APPROVED,
}
