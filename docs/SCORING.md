# DPDP-COMPASS — Compliance Scoring Engine

This document explains the scoring engine implemented in Module 5 (DPDP
Compliance Assessment Engine), per MASTER_PROMPT section 11's explicit
requirement: "Do not use only Passed/Total... create a configurable
scoring engine... the exact formula must be documented."

## 1. Design goal

The platform must never reduce an assessment to a single Passed/Total
ratio, and the formula must be **configurable**, not hard-coded. This is
achieved with two separate, independently swappable pieces:

- **`IComplianceScoringStrategy`** (`DPDP.Application.Modules.Assessments.Scoring`)
  — the *shape* of the formula: which dimensions exist and how they
  combine. Registered once in DI (`DefaultComplianceScoringStrategy`
  today); a future deployment or module could register a different
  implementation without touching any query handler that calls it.
- **`ScoringOptions`** (bound from the `Scoring` configuration section,
  `appsettings.json`) — the *numbers* the default strategy uses: point
  values per answer status, and weight multipliers per risk level. These
  can change per deployment via configuration alone, with no code change
  or redeploy of logic.

Score calculation is always computed **live** from the assessment's
current answers (`GetAssessmentScoreQuery`), never cached on the
`Assessment` row — so a displayed score can never go stale relative to
the underlying answers.

## 2. The five figures shown

Per Module 5's brief, every assessment shows all five of these — never
just one blended number:

| Figure | What it measures |
|---|---|
| **Control Score** | Unweighted average of how well *applicable* controls performed. |
| **Risk-Adjusted Score** | The same average, but weighted by each control's risk level — a failed CRITICAL control hurts more than a failed LOW one. |
| **Evidence Coverage** | Of the mandatory evidence requirements that apply, what fraction actually have evidence attached. |
| **Assessment Coverage** | Of the required questions that apply, what fraction have actually been answered (regardless of the answer's quality). |
| **Overall Score** | The single combined figure — see §4. |

None of these are, or are described as, a legal compliance certification
— see MASTER_PROMPT's introduction and `docs/API.md`/`docs/ARCHITECTURE.md`
for the platform-wide framing constraint this scoring engine operates
under.

## 3. Per-control inputs

For each `AssessmentControl` in an assessment, the scoring engine receives
(`ScoringControlInput`, computed by `AssessmentMapper.ToScoringInput`):

- **Status** — the control's own rollup `AnswerStatus`
  (PASS/PARTIAL/FAIL/NOT_APPLICABLE/NOT_ASSESSED/NEEDS_REVIEW), computed
  by `ControlStatusCalculator` from its *required* questions' answers (see
  §5 below and `docs/ARCHITECTURE.md` Module 5 section).
- **RiskLevel** — the underlying Control's risk level
  (LOW/MEDIUM/HIGH/CRITICAL), from the Module 4 control library.
- **RequiredQuestionCount** / **AnsweredRequiredQuestionCount** — for
  Assessment Coverage.
- **MandatoryEvidenceRequirementCount** / **SatisfiedMandatoryEvidenceCount**
  — for Evidence Coverage. A question's evidence is "satisfied" if at
  least one evidence reference (description and/or URL — see
  `docs/ARCHITECTURE.md` Module 5 section on why there's no file upload
  yet) is attached to its answer.

## 4. The default formula

`DefaultComplianceScoringStrategy` computes:

```
applicable = controls where Status != NOT_APPLICABLE

Control Score       = average(pointValue(c.Status) for c in applicable) × 100
Risk-Adjusted Score  = ( Σ pointValue(c.Status) × riskWeight(c.RiskLevel)
                         ÷ Σ riskWeight(c.RiskLevel) )  for c in applicable, × 100
Assessment Coverage  = ( Σ AnsweredRequiredQuestionCount
                         ÷ Σ RequiredQuestionCount )    for c in applicable, × 100
Evidence Coverage    = ( Σ SatisfiedMandatoryEvidenceCount
                         ÷ Σ MandatoryEvidenceRequirementCount ) for c in applicable, × 100

Overall Score = Risk-Adjusted Score × (Assessment Coverage ÷ 100) × (Evidence Coverage ÷ 100)
```

This mirrors MASTER_PROMPT section 11's own example — "Compliance Score =
Control Effectiveness × Risk Weight × Evidence Confidence × Assessment
Coverage" — folding "control effectiveness" and "risk weight" into the
one Risk-Adjusted Score term, rather than computing control effectiveness
twice (once unweighted, once risk-weighted) and multiplying both into the
final number, which would double-count control performance.

`pointValue(status)` and `riskWeight(riskLevel)` are both read from
`ScoringOptions` (see §6) — nothing here is a hard-coded magic number.

## 5. N/A handling — "vacuous truth"

A dimension with nothing to measure reports **100**, never `null` and
never `0`:

- Every control marked `NOT_APPLICABLE` (excluded from `applicable`
  entirely) → Control Score / Risk-Adjusted Score report 100 ("nothing
  failed among what applies," not "unknown" or "everything failed").
- Zero mandatory evidence requirements exist among applicable controls →
  Evidence Coverage reports 100 (nothing mandatory to satisfy).
- Zero required questions exist among applicable controls → Assessment
  Coverage reports 100 (nothing required to answer).
- An assessment with zero controls at all (e.g. a framework version with
  no ACTIVE controls yet) → every dimension reports 100.

This is a deliberate, documented product choice — not an accident — and
is exercised directly by `DPDP.UnitTests.Application.DefaultComplianceScoringStrategyTests`
(`When_every_control_is_not_applicable_the_score_is_vacuously_100_not_zero`,
`No_mandatory_evidence_requirements_means_full_evidence_coverage_not_zero`,
etc.).

## 6. Configuration

Bound from the `Scoring` section of `appsettings.json` (all optional —
every field defaults to the value shown):

```json
{
  "Scoring": {
    "StatusPointsPass": 1.0,
    "StatusPointsPartial": 0.5,
    "StatusPointsFail": 0.0,
    "StatusPointsNeedsReview": 0.0,
    "StatusPointsNotAssessed": 0.0,
    "RiskWeightLow": 1.0,
    "RiskWeightMedium": 2.0,
    "RiskWeightHigh": 3.0,
    "RiskWeightCritical": 4.0
  }
}
```

Changing these changes every future score calculation immediately — no
migration, no redeploy of the scoring *logic* itself, only a
configuration change. `DPDP.UnitTests.Application.DefaultComplianceScoringStrategyTests.Configuration_changes_the_point_value_awarded_to_a_status`
proves this directly.

## 7. What the control-level rollup status means

`AssessmentControl.Status` (an `AnswerStatus`, reused from Module 4's
control library rather than a duplicate enum) is computed by
`ControlStatusCalculator.CalculateControlStatus`, a pure Domain-layer
function, from the `AnswerStatus` of every **required** question under
that control (non-required questions never affect it):

1. If the control has no required questions at all → `NOT_ASSESSED`.
2. If every required answer is `NOT_APPLICABLE` → the control is
   `NOT_APPLICABLE`.
3. Otherwise, excluding any individually-`NOT_APPLICABLE` answers,
   worst-first precedence decides the result: any `FAIL` wins outright;
   else any `NEEDS_REVIEW`; else any `NOT_ASSESSED` (still incomplete);
   else any `PARTIAL`; else `PASS` only if every remaining required
   answer is `PASS`.

This rollup is recalculated and persisted by `SaveAssessmentAnswerCommand`
and `ReviewAssessmentAnswerCommand` every time an answer under the control
changes — never recomputed lazily on read — so listing/filtering
assessments by control status is a plain column read, not a per-request
aggregation.
