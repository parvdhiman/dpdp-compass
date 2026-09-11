# DPDP-COMPASS — Risk Methodology

This document explains the risk-scoring methodology implemented in
Module 6 (Findings, Risk & Remediation), per the module's explicit
requirement: "Make risk methodology configurable." It mirrors
`docs/SCORING.md`'s approach for compliance scoring — the same
"interface + options class, both resolved via DI" pattern, so a future
deployment can swap the formula without touching any handler.

## 1. The four risk dimensions

Per the Module 6 brief, every `Risk` in the Risk Register carries four
independent inputs:

| Dimension | Scale | What it captures |
|---|---|---|
| **Likelihood** | RARE / UNLIKELY / POSSIBLE / LIKELY / ALMOST_CERTAIN | How probable the risk event is. |
| **Impact** | NEGLIGIBLE / MINOR / MODERATE / MAJOR / SEVERE | How severe the consequence would be if it occurred. |
| **Data Sensitivity** | LOW / MEDIUM / HIGH / CRITICAL | How sensitive the personal data involved is. |
| **Exposure** | LOW / MEDIUM / HIGH / CRITICAL | How exposed/accessible the affected asset or process is. |

Data Sensitivity and Exposure reuse `Compliance.RiskLevel` (the same
four-point scale Module 4's Control library already uses) rather than two
more near-identical enums — see `docs/ARCHITECTURE.md` Module 6 section.

## 2. How the score is calculated

`IRiskScoringStrategy` (`DPDP.Application.Modules.Risks.Scoring`) is the
swappable interface; `DefaultRiskScoringStrategy` is the only
implementation today. Its formula:

1. Each dimension's selected level is normalized to a `0..1` value by its
   ordinal position within its own scale (e.g. `Likelihood.RARE` → `0`,
   `Likelihood.ALMOST_CERTAIN` → `1`; `RiskLevel.LOW` → `0`,
   `RiskLevel.CRITICAL` → `1`).
2. The four normalized values are combined into a single `0..100` score
   using configurable weights:

   ```
   score = ( Likelihood×Lw + Impact×Iw + DataSensitivity×Dw + Exposure×Ew )
           ÷ (Lw + Iw + Dw + Ew) × 100
   ```

3. The score is bucketed into a `RiskLevel` (LOW/MEDIUM/HIGH/CRITICAL —
   the same enum as the risk's own `CalculatedRiskLevel` field) using
   configurable thresholds, evaluated highest-first: `CriticalThreshold`,
   then `HighThreshold`, then `MediumThreshold`; anything below all three
   is `LOW`.

Both the score and the bucketed level are recalculated and persisted
(`Risk.CalculatedRiskScore` / `Risk.CalculatedRiskLevel`) every time a
risk is created or its four inputs are edited — the same "calculate on
write, not on read" precedent `AssessmentControl.Status` set in Module 5 —
so listing/sorting/filtering the Risk Register by level or score is a
plain column read, not a per-request recalculation.

## 3. Configuration

Bound from the `RiskScoring` section of `appsettings.json` (all optional
— every field defaults to the value shown):

```json
{
  "RiskScoring": {
    "LikelihoodWeight": 0.35,
    "ImpactWeight": 0.35,
    "DataSensitivityWeight": 0.15,
    "ExposureWeight": 0.15,
    "CriticalThreshold": 75,
    "HighThreshold": 50,
    "MediumThreshold": 25
  }
}
```

Weights need not sum to 1 — they are normalized internally by their own
total, so e.g. doubling every weight has no effect, but changing one
weight relative to the others shifts how much that dimension drives the
final score. `DPDP.UnitTests.Application.DefaultRiskScoringStrategyTests`
proves both the weight and threshold configuration points actually change
behavior (`Configurable_weights_change_which_dimension_dominates_the_score`,
`Configurable_thresholds_change_which_band_a_score_falls_into`).

## 4. Relationship to Finding.Severity

`Finding.Severity` (CRITICAL/HIGH/MEDIUM/LOW/INFORMATIONAL) and
`Risk.CalculatedRiskLevel` (LOW/MEDIUM/HIGH/CRITICAL) are deliberately
**not the same field** and are not kept in sync automatically:

- A Finding's severity is set directly by whoever raises it (or derived
  from the failed Control's `RiskLevel` when auto-generated from a failed
  assessment — see `docs/ARCHITECTURE.md` Module 6 section) — it reflects
  "how bad is this specific gap."
- A Risk's calculated level reflects the fuller Likelihood/Impact/
  Sensitivity/Exposure picture once someone has assessed it properly —
  "how bad is this risk to the organisation, considering how likely it is
  and what's exposed."

`CreateRiskFromFindingCommand` (the "Finding → Risk" workflow step) uses
the finding's severity only as a **starting point** default for Data
Sensitivity and Exposure — the risk owner is expected to set real
Likelihood/Impact values and refine the rest, not treat the generated risk
as final.

## 5. Non-goals

- This is not a quantitative (financial-loss-modeling) risk methodology —
  it is a standard qualitative Likelihood × Impact matrix, extended with
  the two DPDP-specific dimensions (Data Sensitivity, Exposure) the brief
  requires. A future module could add a quantitative strategy alongside
  this one without changing anything that calls `IRiskScoringStrategy`.
- The methodology does not currently support per-organisation overrides
  (all organisations on one deployment share the same `RiskScoringOptions`
  configuration) — only per-deployment configuration. Per-tenant
  methodology configuration is a reasonable future enhancement, not built
  now.
