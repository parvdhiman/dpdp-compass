namespace DPDP.Domain.Modules.Compliance;

/// <summary>
/// The control's own lifecycle state — distinct from AnswerStatus, which
/// is the verdict of assessing a specific organisation against a control
/// (built in the future Assessment Engine module). Named exactly per the
/// Module 4 brief so the token is stable across API/DB/UI.
/// </summary>
public enum ControlStatus
{
    ACTIVE,
    DRAFT,
    RETIRED,
}
