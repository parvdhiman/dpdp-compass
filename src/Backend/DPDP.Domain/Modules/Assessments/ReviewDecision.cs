namespace DPDP.Domain.Modules.Assessments;

/// <summary>A reviewer's verdict on one review pass — distinct from the final ApprovalDecision, which only a holder of assessments.approve can make.</summary>
public enum ReviewDecision
{
    NEEDS_CHANGES,
    READY_FOR_APPROVAL,
}
