# Compliance Content Governance

This document explains how legal content in DPDP-COMPASS's compliance
framework and control library is sourced, represented, reviewed, and kept
current. It exists because Module 4 introduces the platform's first
content that makes a claim about the law — everything before it was
generic SaaS data (users, organisations). Getting this wrong is a product
credibility and legal-risk problem, not just a bug.

## 1. Scope

This governance process covers everything under `DPDP.Domain.Modules.Compliance`:
`Framework`, `FrameworkVersion`, `LegalReference`, `Requirement`,
`ControlCategory`, `Control`, `AssessmentQuestion`, `EvidenceRequirement`,
and `ControlMapping`. It does not cover assessment answers, findings, or
remediation — those belong to later modules and carry their own,
organisation-specific governance (not a legal-accuracy concern in the same
way).

## 2. Authoritative sources used for the seed data

The seed data shipped in Module 4
(`DPDP.Infrastructure/Persistence/Seed/DpdpActSeedData.cs`) is derived
from:

- **The Digital Personal Data Protection Act, 2023** (Act No. 22 of 2023),
  as passed by Parliament and assented to on 11 August 2023 — section
  numbers, section titles, and the substantive obligations of Sections 4
  through 13 and Section 16.
- Structural facts only (act number, assent date, section numbering) are
  treated as high-confidence and cited directly.

The seed data deliberately does **not** cover:

- **The Digital Personal Data Protection Rules, 2025.** As of when this
  module was built, the Rules had not been finalised/notified in a form
  this codebase could cite with confidence on section numbering and exact
  text. Rather than guess, `Framework`/`FrameworkVersion` rows exist for
  the Rules (so the schema and versioning relationship are real and
  testable), but **zero** `LegalReference`/`Requirement`/`Control` rows are
  seeded under it. This is a deliberate empty shell, not an oversight —
  see §5.
- Any secondary source (case law, guidance notes, industry commentary,
  another vendor's compliance mapping). Only the Act itself is a source of
  truth for this seed data.

## 3. What "paraphrase, not verbatim" means

Every `LegalReference.SummaryText` and every `Control`/`Requirement`
description in the seed data is a **plain-language paraphrase** of the
Act's obligation, written by a person, not a copy-paste of statutory text.
This is stated explicitly in the doc comments on `LegalReference.SummaryText`
and in `DpdpActSeedData.cs` itself. Two reasons:

1. **Accuracy under change**: paraphrased, structured content
   (Requirement → Control → Question) is what makes the platform's
   assessment engine possible; a verbatim block of statute is not
   assessable.
2. **Attribution discipline**: forcing every summary to be a paraphrase,
   with the actual citation kept in a separate, mandatory field, prevents
   the two from silently drifting apart — the reader always has the real
   citation next to the plain-language claim about what it says.

**`LegalReference.SourceCitation` is a required field with no default.**
Every legal reference must contain its source — this is enforced by
`CreateLegalReferenceCommandValidator`/`UpdateLegalReferenceCommandValidator`
(`NotEmpty()`), not just a convention. The seed data's citations follow the
format `"Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023),
Section N"`.

## 4. The two status axes, and what each one promises

Every piece of legal content carries **two independent** status fields.
Conflating them was the single easiest mistake to make when designing this
module, so they are kept deliberately separate:

| Axis | Enum | Question it answers | Who changes it |
|---|---|---|---|
| `ControlStatus` | `DRAFT` / `ACTIVE` / `RETIRED` | Is this control currently in operational use — should organisations see and answer it? | Super Administrator, via `ActivateControlCommand`/`RetireControlCommand` |
| `ContentReviewStatus` | `DRAFT` / `LEGAL_REVIEWED` / `APPROVED` | Has a human with legal authority verified the wording is accurate? | Super Administrator, via the Update commands' `reviewStatus` field |

A control's `ControlStatus` and `ContentReviewStatus` are allowed to be in
any combination. In particular, **the Module 4 seed data ships all 13 DPDP
Act controls as `Status = ACTIVE`, `ReviewStatus = DRAFT`.** This is a
deliberate choice, not a placeholder that got forgotten:

- Shipping `DRAFT`/`DRAFT` would make the entire control library invisible
  to every organisation (see §5's visibility rule), defeating the purpose
  of building it.
- Shipping `ACTIVE`/`APPROVED` would overstate how rigorously the content
  has been checked — nobody with legal review authority has signed off on
  the paraphrased wording yet.
- `ACTIVE`/`DRAFT` is the honest middle: the control is usable today (it
  is operationally sound and derived carefully from the Act), and its
  `ReviewStatus` is visible everywhere as a permanent, non-hideable flag so
  nobody mistakes "seeded from the Act" for "legally sign-off complete."

`ContentReviewStatus` is never used to hide content from anyone —
including from organisations — it is metadata only. Hiding is controlled
exclusively by `ControlStatus`.

## 5. The organisation-facing visibility rule

Any caller who is **not** a Super Administrator always sees only
`ControlStatus.ACTIVE` content, regardless of what status filter (if any)
they request. This is enforced inside the query handlers themselves
(`GetControlsQuery`, `GetControlByIdQuery`, `GetQuestionsQuery`,
`GetEvidenceRequirementsQuery`) — not a global EF Core query filter —
because a Super Administrator legitimately needs to see `DRAFT` and
`RETIRED` content (to finish authoring a control, or to review something
being retired) through the exact same endpoints. One query per entity
serves both the admin control-library view and the organisation-facing
read-only view; see `docs/API.md` for the endpoint list.

Because of this rule, the empty DPDP Rules, 2025 framework shell (§2) is
completely invisible to any effect on organisations — there are no
`ACTIVE` controls under it, so no organisation is ever shown incomplete or
unverified Rules-derived content. Framework and FrameworkVersion listings
themselves are visible to everyone (they are process/versioning metadata,
not the accuracy-sensitive part), but that framework's control tree stays
empty until real content is added under it.

## 6. Versioning: what changes require a new `FrameworkVersion`

A new `FrameworkVersion` is created whenever the underlying framework
(Act, Rules, or any future framework this platform supports) changes in a
way that affects sections, obligations, or their numbering. Every version
carries:

- `VersionLabel` — a short human identifier (e.g. `"2023"`).
- `OfficialCitation` — e.g. `"Act No. 22 of 2023"`.
- `PublicationDate` — when the version was published/notified.
- `EffectiveDate` — when the version's obligations actually take legal
  effect. **This is deliberately nullable and left `null` in the seed
  data for the Act.** The DPDP Act, 2023 was assented to on 11 August 2023
  but commences via separate notification, section-by-section, on dates
  the Central Government has not (as of this module's construction)
  fully announced. Guessing an effective date here would be exactly the
  kind of false legal claim this module is built to avoid — a `null`
  `EffectiveDate` with a `ChangeSummary` explaining why is the honest
  state, not a bug.
- `SourceUrl` — a link to the official text, when available.
- `ChangeSummary` — free text explaining what changed from the prior
  version (or, for a first version, its provenance).
- `ReviewStatus` — same `ContentReviewStatus` enum as above.
- `IsCurrent` — exactly one `TRUE` per `Framework` at a time, flipped
  atomically by `ActivateFrameworkVersionCommand` (see
  `docs/ARCHITECTURE.md` §14).

Only a Super Administrator can create or activate a version
(`controls.manage` plus the in-handler `IsSuperAdministrator` check, same
pattern as every other write in this module).

## 7. The review workflow (process, not yet automated)

1. **Draft**: a Super Administrator adds or edits a `Framework`/
   `FrameworkVersion`/`LegalReference`/`Requirement`/`Control` via the
   admin APIs. New content always starts `ContentReviewStatus.DRAFT`.
2. **Legal review**: a person with legal authority over this platform's
   content (not necessarily an engineer) reviews the paraphrased wording
   against the actual statutory text and the `SourceCitation`. If
   accurate, its `ReviewStatus` moves to `LEGAL_REVIEWED` via the
   corresponding Update command.
3. **Approval**: a second sign-off (e.g. a compliance lead or legal
   counsel with final authority) moves it to `APPROVED`. What
   organisation-visible behavior (if any) should ever key off `APPROVED`
   vs. `LEGAL_REVIEWED` is intentionally left to product/legal to define —
   this module does not gate any user-facing behavior on
   `ContentReviewStatus` today (see §5); it exists as an auditable trail.
4. **Re-review on change**: any edit to a `LEGAL_REVIEWED`/`APPROVED` item
   should reset it to `DRAFT` and re-run the cycle. This module does not
   currently automate that reset — `UpdateControlCommand` and friends take
   `reviewStatus` as an explicit input rather than forcing it back to
   `DRAFT` on every edit, so it is a **process discipline** for whoever
   operates the admin UI today, not (yet) a system-enforced invariant. This
   is a known gap, tracked for a future module once there are multiple
   real reviewers using the admin UI rather than a single seed import.

There is currently no in-app workflow/notification for step 2–3 (no
"pending review" queue, no reviewer role distinct from Super
Administrator). This is acceptable for Module 4's scope — the entire seed
library is one reviewable file (§8) — but should be revisited before this
platform's content is maintained by more than one person.

## 8. Where to make a change

**All seed content is one file**:
`DPDP.Infrastructure/Persistence/Seed/DpdpActSeedData.cs`. It is deliberately
*not* scattered across the nine `IEntityTypeConfiguration<T>` classes that
reference it — each configuration's `HasData(...)` call projects from this
one file, so a legal-content reviewer only ever needs to read one file,
top to bottom, to see everything the platform currently asserts about the
law. Every new Framework/Control/Question/Evidence addition should be
added here first, as a new entry in the relevant array, with a
`DeterministicGuid.Create(...)`-derived id so the resulting EF Core
migration is purely additive (see `docs/DATABASE.md`).

Changing seeded content after it has shipped requires a new EF Core
migration (seed data lives in migrations via `HasData`, not a runtime
seeding step) — see `docs/DATABASE.md` for the migration conventions this
project follows.

## 9. Explicit non-goals of this module

- No claim in this codebase should ever be read as legal advice. This
  platform assesses against a structured interpretation of the DPDP Act,
  authored by this project, not an official government interpretation.
- No content is sourced from anything other than the Act itself (§2) —
  no blog posts, no other vendors' frameworks, no AI-generated summaries
  of the Act. Every `SummaryText`/`Description` in the seed data was
  written by directly reading the cited section.
- The DPDP Rules, 2025 are represented but empty until they can be sourced
  with the same confidence as the Act (§2, §5) — this module does not
  invent Rules content to fill the shell.
