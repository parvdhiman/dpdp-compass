using DPDP.Domain.Modules.Compliance;

namespace DPDP.Domain.Modules.Assessments;

/// <summary>
/// Pure rollup logic: given the answer statuses of a control's *required*
/// questions, decides the control's own AnswerStatus. No database access,
/// no framework dependency — intentionally a plain static function so it
/// can be unit-tested in isolation (score calculation / N/A handling /
/// partial answers are explicit Module 5 test requirements). Called by
/// SaveAssessmentAnswerCommand every time one of a control's answers
/// changes; the result is persisted on AssessmentControl.Status, not
/// recomputed on every read.
/// </summary>
public static class ControlStatusCalculator
{
    /// <param name="requiredAnswerStatuses">
    /// The Status of every AssessmentAnswer belonging to this control whose
    /// question has IsRequired = true. Non-required questions never affect
    /// the control's rollup status.
    /// </param>
    public static AnswerStatus CalculateControlStatus(IReadOnlyCollection<AnswerStatus> requiredAnswerStatuses)
    {
        if (requiredAnswerStatuses.Count == 0)
        {
            // No required question exists for this control at all — there is
            // nothing to have an opinion about yet. Not the same as every
            // required question being explicitly marked NOT_APPLICABLE.
            return AnswerStatus.NOT_ASSESSED;
        }

        var applicable = requiredAnswerStatuses.Where(s => s != AnswerStatus.NOT_APPLICABLE).ToList();

        if (applicable.Count == 0)
        {
            // Every required question was explicitly marked not applicable —
            // the control itself is not applicable, not a failure to assess.
            return AnswerStatus.NOT_APPLICABLE;
        }

        // Worst-first precedence: an explicit failure always surfaces first,
        // then anything a human flagged for review, then plain incompleteness,
        // then partial credit — a control is only PASS if every applicable
        // required answer is PASS.
        if (applicable.Contains(AnswerStatus.FAIL)) return AnswerStatus.FAIL;
        if (applicable.Contains(AnswerStatus.NEEDS_REVIEW)) return AnswerStatus.NEEDS_REVIEW;
        if (applicable.Contains(AnswerStatus.NOT_ASSESSED)) return AnswerStatus.NOT_ASSESSED;
        if (applicable.Contains(AnswerStatus.PARTIAL)) return AnswerStatus.PARTIAL;

        return AnswerStatus.PASS;
    }
}
