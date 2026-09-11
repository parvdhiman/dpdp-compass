# DPDP-COMPASS — Consent & Privacy Operations

This document explains Module 10: Privacy Notices, Consent Purposes,
Consent Records, Consent Withdrawal, Data Principal Requests, and
Grievances — the data-minimisation design that runs through every
entity, the versioning model for notices, the append-only design of
consent evidence, the unification of requests and grievances into one
workflow, the SLA-configurability rationale, and the "portal-ready"
architecture.

## 1. `DataPrincipal` is a pointer, not a person record

The brief is explicit: "Never store unnecessary raw personal data" and
"Support external identity/reference IDs." Read together, these mean
this module should not become a second database of names, emails, and
phone numbers sitting alongside whatever system of record the
organisation already has for its customers, employees, or other data
subjects.

`DataPrincipal` therefore has exactly one identifying field:
`ExternalReferenceId` — a token that means something in the
organisation's *own* system (a customer ID, an employee ID, a CRM
record key). It also carries an optional `ReferenceCategory` (reusing
Module 9's `DataInventory.DataSubjectCategory` enum rather than
duplicating it — see docs/DATA_INVENTORY.md) and free-text `Notes`. No
name. No email. No phone number. Looking up a `DataPrincipal`'s actual
contact details is the organisation's own system's job; this platform
only needs a stable handle to correlate consent and request records to
"the same person" over time.

This is a deliberate architectural choice, not an oversight: it is the
only way to satisfy "never store unnecessary raw personal data" for an
entity whose entire purpose is to represent a person, without leaving
the field vague enough to be reinterpreted as "store whatever's
convenient."

`DataPrincipalRequest` is different, and intentionally so (see section
4): it carries `RequesterName`, `RequesterContactEmail`, and
`RequesterContactPhone`, because processing and corresponding about
*that specific request* requires knowing who to write back to. This
is not a second copy of `DataPrincipal`'s data — it is the identity
claimed at intake for one request's own lifecycle, matched to a
`DataPrincipal` reference only once verified.

## 2. `PrivacyNotice` versioning: `Code` identifies, `Version` distinguishes

A privacy notice changes over time — new purposes get added, wording
gets revised for a regulatory update — and an organisation needs to be
able to say precisely which text a given consent was captured against,
even years later after the notice has been revised twice more.

`PrivacyNotice.Code` identifies *which* notice this is (e.g.
`PRIVACY-POLICY`, `COOKIE-NOTICE`) across all of its revisions.
`PrivacyNotice.Version` identifies *which revision* (`1.0`, `2.1`, ...).
The pair `(Code, Version)` is unique. A `ConsentRecord.NoticeVersionId`
always points at one specific `PrivacyNotice` row — a specific version —
never just a code, so "what did this person agree to" is always an
exact, immutable answer.

Because of this, a published notice's content is never edited in place:
`UpdatePrivacyNoticeCommand` only permits changes while the notice is
still `DRAFT`. Once `PUBLISHED`, a correction means creating a *new*
`PrivacyNotice` row with the same `Code` and a new `Version`, going
through its own DRAFT → APPROVED → PUBLISHED workflow. The status
machine (`PrivacyNoticeStatusTransitions`) is strictly linear —
DRAFT → APPROVED → PUBLISHED → ARCHIVED, no reopening, no skipping —
because a notice that people have already relied on to give consent
should never silently change meaning underneath them.

Approval and publishing are split into two actions
(`PrivacyNoticesApprove` vs `PrivacyNoticesManage`) so that the person
who drafts a notice cannot also be the one who signs off on it — the
same separation-of-duties pattern as Module 9's Processing Activity
review/approve split.

## 3. `ConsentRecord` is compliance evidence, not a mutable row

`ConsentRecord` is the one aggregate root in this module that is
deliberately **not** `ISoftDeletable` and has no delete endpoint. A
consent record is evidence that a data principal did or did not agree
to something at a point in time — deleting or freely editing it would
destroy the very thing this module exists to prove. It follows the same
"append-only, state changes only via dedicated actions" shape as
`AuditLog`.

Its lifecycle only ever moves forward, enforced by
`ConsentStatusTransitions`:

```
GRANTED → WITHDRAWN
GRANTED → EXPIRED
GRANTED → REVOKED
```

`WITHDRAWN`, `EXPIRED`, and `REVOKED` are all terminal — a consent is
never resurrected; a new consent is captured instead if the data
principal opts back in.

**Withdraw vs. Revoke** are deliberately two different commands, even
though both move `GRANTED` to a terminal status:

- `WithdrawConsentCommand` represents the **data principal's own
  action** — they changed their mind and withdrew consent themselves.
- `RevokeConsentCommand` represents an **organisation-initiated
  invalidation** — e.g. the underlying notice was retracted, or a legal
  issue means the consent can no longer be relied upon. It requires a
  `Reason`, which `WithdrawConsentCommand` does not, because an
  organisation invalidating someone's consent on their behalf needs to
  be justified and auditable in a way that a person withdrawing their
  own consent does not.

Both are recorded distinctly in the audit log
(`consentprivacy.consent_withdrawn` vs `consentprivacy.consent_revoked`)
so a compliance report can tell the two apart.

There is no scheduler in this codebase (see Module 7/8 precedent), so
expiry is handled the same way evidence expiry is: an on-demand batch
command, `MarkConsentExpiredCommand`, transitions any `GRANTED` consent
whose `ExpiresAt` has passed to `EXPIRED`. An organisation would wire
this to a periodic job in its own infrastructure; this module provides
the operation, not the trigger.

## 4. Data Principal Requests unify requests and grievances

The brief lists "Data Principal Requests" and "Grievances" as separate
top-level items, but gives them one identical field shape and one
identical seven-state workflow. Rather than build two entities that
would immediately diverge into copy-pasted code, "Grievance" is simply
`DataPrincipalRequestType.GRIEVANCE` on the same `DataPrincipalRequest`
entity — the fourth time this codebase has reconciled a brief's
originally-separate roadmap items into one entity, after Modules 6, 8,
and 9 (see their respective docs).

`RequestType` covers `ACCESS`, `CORRECTION`, `ERASURE`,
`WITHDRAW_CONSENT`, `GRIEVANCE`, and `OTHER`. All six share the same
workflow, enforced by `DataPrincipalRequestStatusTransitions`:

```
REQUESTED → IDENTITY_VERIFICATION → IN_PROGRESS ⇄ AWAITING_INFORMATION
IN_PROGRESS → COMPLETED | REJECTED
COMPLETED | REJECTED → CLOSED
```

`COMPLETED`, `REJECTED`, and `CLOSED` are all reachable only forward;
`CLOSED` is the only fully terminal status.

`RequesterName`/`RequesterContactEmail`/`RequesterContactPhone` are the
identity claimed at intake — see section 1 for why this is not a
`DataPrincipal`-duplicating decision. `DataPrincipalId` starts `null`
and is set once `VerifyDataPrincipalRequestIdentityCommand` matches the
claimed identity to a known `DataPrincipal` reference (or is left
matched to nothing, for a request from someone with no prior record).

## 5. SLA due dates are configuration, never a literal

The brief is explicit: "Do not hard-code legal deadlines without
verified legal source/configuration." No statutory deadline for
responding to a Data Principal Request is asserted anywhere in this
codebase's source — the same "empty shell until a verified legal source
exists" precedent as the DPDP Rules, 2025 content in
docs/COMPLIANCE_CONTENT_GOVERNANCE.md.

Instead, `SlaPolicy` is a fully organisation-managed catalog:

- `RequestType` (nullable): a policy scoped to one specific request
  type, or `null` for a catch-all that applies to any type with no more
  specific policy configured.
- `ResponseDueDays`: the number of days, sourced from the
  organisation's own legal counsel — this platform has no opinion on
  what the right number is.
- `IsActive`: only one active policy may exist per `RequestType` at a
  time (enforced by a `ConflictException` in
  `CreateSlaPolicyCommand`/`UpdateSlaPolicyCommand`), so "which policy
  applies" is never ambiguous.

No default policy is seeded. `CreateDataPrincipalRequestCommand` calls
`SlaPolicyResolver`, which looks for an active policy matching the
request's exact `RequestType` first, falls back to the catch-all
(`RequestType == null`) policy, and — if neither exists — leaves
`DueAt` as `null`. A request with no configured SLA is not silently
assigned an invented deadline; it is visibly unSLA'd, which is the
correct state until the organisation configures one.

`GetDataPrincipalRequestSlaSummaryQuery` computes overdue/due-soon/
unconfigured counts on demand (never persisted, same rationale as
`MarkConsentExpiredCommand`) for a dashboard widget — no assumption
about what "due soon" means beyond a fixed 48-hour window used purely
for that widget's own bucketing, not a compliance deadline.

## 6. Portal-ready architecture

The brief asks for a "portal-ready architecture" without asking this
module to build an actual public-facing portal. That is satisfied
architecturally, not by standing up unauthenticated endpoints:

- Every data-principal-facing field (`DataPrincipal.ExternalReferenceId`,
  `DataPrincipalRequest.RequesterName`/`RequesterContactEmail`/
  `RequesterContactPhone`) is plain data — not a foreign key into this
  platform's internal `User`/RBAC system. A future public self-service
  portal (where a data principal submits their own request or manages
  their own consents) could write directly into these tables without
  any redesign of the domain model, because nothing here assumes the
  caller is an authenticated internal `User`.
- `CreateDataPrincipalRequestCommand` and `CreateConsentCommand` do not
  require the caller to already know an internal user or role — they
  only require organisation context, which a portal's own tenant
  resolution (e.g. by subdomain or API key) could supply.
- The workflow commands (`AssignDataPrincipalRequestCommand`,
  `VerifyDataPrincipalRequestIdentityCommand`, etc.) are kept separate
  from creation specifically so that a portal's "submit a request" flow
  and an internal team's "process a request" flow can be wired to
  different authorization boundaries without touching the same code
  path.

No unauthenticated endpoint exists yet — every endpoint in this module
requires authentication and a permission, same as every other module.
Building the actual portal (its own authentication model, rate
limiting, CAPTCHA, etc.) is future scope; this module only ensures the
domain model doesn't have to be reshaped when that scope arrives.

## 7. Permissions

| Permission | Grants |
|---|---|
| `privacynotices.read` / `.manage` / `.approve` | View / create-edit-publish-archive / approve Privacy Notices |
| `consentpurposes.read` / `.manage` | View / manage the Consent Purpose catalogue |
| `consent.read` / `.manage` | View / capture-withdraw-revoke-expire Consent Records |
| `dataprincipals.read` / `.manage` | View / manage Data Principal references |
| `datarequests.read` / `.manage` | View / manage Data Principal Requests, Grievances, and SLA Policies |

`SlaPolicy` shares the `datarequests.*` permission pair rather than
getting its own, since it exists purely to govern Data Principal
Request due dates.

The **Privacy Officer** role holds every `.manage` permission in this
module — it is the day-to-day operator. The **Compliance Officer**
holds only `privacynotices.approve` plus read access to everything else
— the same separation-of-duties shape as its role in Module 9's
Processing Activity approval.
