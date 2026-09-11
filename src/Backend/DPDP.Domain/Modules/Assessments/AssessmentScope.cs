using DPDP.Domain.Common;
using DPDP.Domain.Modules.Organisations;

namespace DPDP.Domain.Modules.Assessments;

/// <summary>
/// Which part of the organisation this assessment covers — descriptive
/// scope for reporting/context, not a filter that changes which controls
/// apply (control applicability is a judgment call made per-control during
/// the questionnaire, via AssessmentControl.Status = NOT_APPLICABLE; see
/// docs/ARCHITECTURE.md Module 5 section for why an automated applicability
/// rule engine was deliberately not built). A row with both ids null means
/// "whole organisation."
/// </summary>
public sealed class AssessmentScope : Entity, ITenantScoped
{
    public Guid OrganisationId { get; set; }

    public Guid AssessmentId { get; set; }
    public Assessment Assessment { get; set; } = null!;

    public Guid? BusinessUnitId { get; set; }
    public BusinessUnit? BusinessUnit { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
