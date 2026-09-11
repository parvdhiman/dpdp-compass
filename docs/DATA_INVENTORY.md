# DPDP-COMPASS — Data Inventory & Processing Activities

This document explains Module 9: the Data Inventory, the Processing
Activity Register (this platform's Record of Processing Activities), and
Data Flow metadata — the naming decisions made to avoid collisions with
earlier modules, the relationship model, the approval workflow, and the
search/filter/export surface.

## 1. Naming collisions avoided

The brief's literal entity names collide with two earlier modules;
both were renamed, following the same precedent as Module 5's
`AssessmentControlQuestion` and Module 9's own `ItSystem`:

- **"System" → `ItSystem`.** "System" collides with the .NET base
  namespace — every file using the entity unqualified would otherwise
  need to fully qualify ordinary BCL types.
- **"Data Source" → `DataCollectionSource`.** Module 8 already owns
  `DataSource` for a technical connection to a customer database/
  filesystem used for discovery scans — an unrelated concept from "where
  a piece of personal data was originally collected" (a web form, a phone
  call, a third party). Reusing the name would have made two completely
  different things share one identifier across the codebase.
- **"Data Element" folded into `DataInventoryItem.DataElementName`, not a
  new entity.** Module 8 already owns `DataElement` for one discovered
  database column. Rather than collide, `DataInventoryItem` carries the
  element's name as a plain string field, plus an optional
  `DiscoveredDataElementId` bridge to a real Module 8 `DataElement` when
  the inventory entry originated from an automated discovery scan — most
  entries won't have one, since discovery only covers data sources an
  operator has registered, but the link exists for the entries that do.

## 2. The two entry points, six shared catalogues

`DataInventoryItem` (what personal data exists, and where) and
`ProcessingActivity` (why it's processed, and by which systems/parties)
are two independent aggregate roots that both reference six small,
organisation-managed catalogues:

```
DataCategory   ItSystem   DataCollectionSource   Processor   Recipient   RetentionPolicy
```

This mirrors the brief's relationship chain:

```
Processing Activity → Data Categories → Systems → Data Sources → Processors → Recipients → Retention Policy
```

`DataInventoryItem` links to each catalogue with a single nullable FK
(one category, one system, one source, one processor, one retention
policy per inventory row) plus a free-text `SharingDescription` for "who
this data is shared with" — sharing didn't need its own catalogue entity
since it's descriptive, not something inventory rows are individually
filtered by in the current brief. `ProcessingActivity` instead has a
genuine **many-to-many** relationship to five of the six catalogues
(`DataCategory`, `ItSystem`, `DataCollectionSource`, `Recipient`,
`Processor` — implicit EF Core join tables, no dedicated join-entity
class since no extra column is needed on the relationship itself) plus a
single nullable `RetentionPolicy` FK, since one processing activity
typically touches several systems/categories/parties at once but follows
one retention policy.

All nine entities (six catalogues + `DataInventoryItem` +
`ProcessingActivity` + `DataFlow`) are independent, tenant-scoped
aggregate roots — none cascade-deletes another; they cross-reference each
other via nullable FKs and many-to-many joins. A catalogue entry that is
still referenced by a `DataInventoryItem` or `ProcessingActivity` cannot
be deleted (409 Conflict) — see each `Delete*CommandHandler`.

## 3. Processing Activity workflow

The brief's "Create, Draft, Review, Approve, Archive" maps to:

```
DRAFT → IN_REVIEW → APPROVED → ARCHIVED
  ^          |           |
  └──────────┘           |
  (send back)      (reopen for edits)
  └───────────────────────┘
```

`ProcessingActivityStatusTransitions` (pure, unit-tested, the same shape
as `FindingStatusTransitions`/`EvidenceStatusTransitions`) defines which
transitions are structurally possible; each command handler then narrows
further with an explicit precondition so the two paths that both land on
`DRAFT` stay semantically distinct:

- **`SendProcessingActivityBackToDraftCommand`** (`processingactivities.review`)
  only succeeds from `IN_REVIEW` — a reviewer rejecting a submission, with
  required comments explaining why.
- **`ReopenProcessingActivityCommand`** (`processingactivities.manage`)
  only succeeds from `APPROVED` — the owner deciding an already-approved
  record needs updating, no comments required.

Review workflow fields (`SubmittedForReviewAt`, `ReviewedAt`,
`ReviewedBy`, `ReviewComments`, `ApprovedAt`, `ApprovedBy`, `ArchivedAt`,
`ArchivedBy`) are flattened directly onto `ProcessingActivity` rather than
modeled as separate child entities — contrast Module 5's
`AssessmentReview`/`AssessmentApproval`. This is a deliberate
simplification: this workflow is one review→approve chain per submission
cycle, not iterative multi-round review with its own audit trail
requirement: the brief's is a lighter-weight, one-reviewer-decision
workflow.

### Separation of duties

`processingactivities.manage` (create/edit/submit/archive/reopen) and
`processingactivities.review`/`.approve` (send-back/approve) are held by
disjoint role sets in the default templates — Privacy Officer manages;
Compliance Officer reviews and approves. The same anti-self-approval
shape Module 7 established between evidence upload and review. A
Processing Activity's preparer can never approve their own submission —
asserted directly by an API test.

## 4. `DataSubjectCategories` and `SecurityControls` are JSON tag lists

`ProcessingActivity.DataSubjectCategoriesJson` and `.SecurityControlsJson`
are lightweight JSON array columns, the same pattern as
`DataAsset.IndexesJson` (Module 8) and `RemediationTask.EvidenceJson`
(Module 6) — small, per-record tag sets that don't need their own
manageable, reusable catalogue entity the way `DataCategory`/`ItSystem`/
etc. do. Data subject categories are a fixed enum
(`CUSTOMER`/`EMPLOYEE`/`VENDOR`/`PROSPECT`/`JOB_APPLICANT`/`MINOR`/`OTHER`);
security controls are free-text strings (an organisation's actual control
names vary too much to enumerate).

## 5. Data Flow metadata

`DataFlow` records that a movement of data is known to exist — never an
actual data pipe or integration. Each side (`From*`/`To*`) is either a
catalogue reference (an `ItSystem`/`DataCollectionSource` on the "from"
side; an `ItSystem`/`Processor`/`Recipient` on the "to" side) or, when no
catalogue entry fits, a required free-text description
(`FromDescription`/`ToDescription`) that always renders even when every
FK is null. `IsCrossBorder`/`CrossBorderCountry` give basic visibility
into cross-border transfers — a `crossBorderCountry` is required whenever
`isCrossBorder` is true (`CreateDataFlowCommandValidator`).

`DataFlow` optionally links to one `ProcessingActivity` (which activity
this flow supports) and one `DataCategory` (what kind of data flows) —
both nullable, since a flow can be recorded standalone ahead of the
processing activity that will eventually reference it.

## 6. Search, filter, and export

Every list endpoint supports the filters its dashboard tab actually uses
(search by name, filter by category/system/processor/risk-level/status/
owner). `GET /api/v1/data-inventory/export` and
`GET /api/v1/processing-activities/export` return `text/csv` — the same
filters as the paginated list, applied unpaginated, via a small
dependency-free `CsvWriter` (`DPDP.Application.Common.Csv`, RFC 4180
quoting) rather than pulling in a CSV library for something this small.
This is this platform's first CSV export; a future module needing one
reuses `CsvWriter` rather than reinventing it.

## 7. Audit logging

Every state-changing action across all nine entities is audit-logged —
catalogue create/update/delete, data inventory item create/update/delete,
processing activity create/update/delete/submit/approve/send-back/
archive/reopen, and data flow create/update/delete. Verified directly
against the `audit_logs` table by
`DataInventoryApiTests` (not just "the endpoint returned 200").
