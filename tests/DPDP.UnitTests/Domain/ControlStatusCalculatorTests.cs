using DPDP.Domain.Modules.Assessments;
using DPDP.Domain.Modules.Compliance;
using Xunit;

namespace DPDP.UnitTests.Domain;

public class ControlStatusCalculatorTests
{
    [Fact]
    public void No_required_questions_yields_not_assessed()
    {
        var result = ControlStatusCalculator.CalculateControlStatus([]);

        Assert.Equal(AnswerStatus.NOT_ASSESSED, result);
    }

    [Fact]
    public void All_required_answers_not_applicable_makes_the_control_not_applicable()
    {
        var result = ControlStatusCalculator.CalculateControlStatus([AnswerStatus.NOT_APPLICABLE, AnswerStatus.NOT_APPLICABLE]);

        Assert.Equal(AnswerStatus.NOT_APPLICABLE, result);
    }

    [Fact]
    public void A_single_not_applicable_answer_among_others_is_excluded_not_blocking()
    {
        var result = ControlStatusCalculator.CalculateControlStatus([AnswerStatus.PASS, AnswerStatus.NOT_APPLICABLE, AnswerStatus.PASS]);

        Assert.Equal(AnswerStatus.PASS, result);
    }

    [Fact]
    public void All_pass_yields_pass()
    {
        var result = ControlStatusCalculator.CalculateControlStatus([AnswerStatus.PASS, AnswerStatus.PASS]);

        Assert.Equal(AnswerStatus.PASS, result);
    }

    [Fact]
    public void Any_fail_wins_over_everything_else()
    {
        var result = ControlStatusCalculator.CalculateControlStatus(
            [AnswerStatus.PASS, AnswerStatus.NEEDS_REVIEW, AnswerStatus.NOT_ASSESSED, AnswerStatus.PARTIAL, AnswerStatus.FAIL]);

        Assert.Equal(AnswerStatus.FAIL, result);
    }

    [Fact]
    public void Needs_review_wins_over_not_assessed_and_partial_when_there_is_no_fail()
    {
        var result = ControlStatusCalculator.CalculateControlStatus([AnswerStatus.PASS, AnswerStatus.NEEDS_REVIEW, AnswerStatus.PARTIAL]);

        Assert.Equal(AnswerStatus.NEEDS_REVIEW, result);
    }

    [Fact]
    public void An_unanswered_required_question_keeps_the_control_not_assessed_even_if_others_passed()
    {
        var result = ControlStatusCalculator.CalculateControlStatus([AnswerStatus.PASS, AnswerStatus.NOT_ASSESSED]);

        Assert.Equal(AnswerStatus.NOT_ASSESSED, result);
    }

    [Fact]
    public void A_partial_answer_with_no_fail_or_incomplete_yields_partial()
    {
        var result = ControlStatusCalculator.CalculateControlStatus([AnswerStatus.PASS, AnswerStatus.PARTIAL]);

        Assert.Equal(AnswerStatus.PARTIAL, result);
    }
}
