# DPDP-COMPASS — API

## 1. Conventions

- **Base path:** `/api/v1/...`. Version bump (`/api/v2`) only on a breaking
  change; additive changes stay on v1.
- **Verbs:** `GET` (read), `POST` (create / non-idempotent action),
  `PUT`/`PATCH` (full/partial update), `DELETE` (remove — soft-delete under
  the hood where the entity supports it, per `DATABASE.md`).
- **DTOs only.** No EF Core entity is ever serialized directly in a
  response or accepted directly as a request body.
- **Auth:** `Authorization: Bearer <jwt>` on every endpoint except
  `POST /api/v1/auth/login`, `POST /api/v1/auth/refresh`, and
  `GET /health*`.
- **Tenant scoping:** never accept a tenant/organisation id from the caller
  to determine *whose* data to return — it comes from the JWT's `org_id`
  claim (see `ARCHITECTURE.md` §4). Where a Super Administrator screen
  legitimately needs to target a specific organisation, that id is an
  explicit, permission-gated route parameter (e.g.
  `GET /api/v1/admin/organisations/{organisationId}/users`), never an
  implicit override of the normal scoping.
- **Correlation:** every request/response carries `X-Correlation-Id`
  (caller-supplied or server-generated); it is echoed back and written to
  `audit_logs.correlation_id` and Serilog scope.
- **Pagination:** list endpoints accept `?page=1&pageSize=25` (default 25,
  max 100) and return:
  ```json
  {
    "items": [ ... ],
    "page": 1,
    "pageSize": 25,
    "totalCount": 137,
    "totalPages": 6
  }
  ```
- **Sorting/filtering/search:** `?sort=fieldName` / `?sort=-fieldName`
  (descending), `?filter[field]=value`, `?q=free text` where a module
  supports search. Allowed sort/filter fields are an explicit allowlist per
  endpoint — never a raw pass-through to the ORM (prevents enumeration of
  internal column names and injection via dynamic sort).
- **Errors:** RFC 7807 `application/problem+json` for every 4xx/5xx:
  ```json
  {
    "type": "https://dpdp-compass/errors/validation-failed",
    "title": "Validation failed",
    "status": 400,
    "detail": "One or more fields are invalid.",
    "instance": "/api/v1/organisations",
    "correlationId": "b3f1...",
    "errors": {
      "name": ["Name is required."]
    }
  }
  ```
  Production environments never include stack traces or internal
  exception messages in `detail` (MASTER_PROMPT §7/§14).
- **Status codes:** `200` read/update success, `201` create (with
  `Location` header), `204` delete success, `400` validation, `401` not
  authenticated, `403` authenticated but not authorized (permission or
  tenant mismatch — same code for both, to avoid leaking which one), `404`
  not found (or not visible to this tenant — same response either way),
  `409` conflict (e.g. concurrency token mismatch, duplicate email), `429`
  rate limited, `500` unhandled (logged, generic body only).
- **OpenAPI/Swagger:** generated from the Minimal API endpoint groups,
  served at `/swagger` in non-Production environments only.

## 2. Endpoint Surface (as implemented through Module 10)

Permission keys below are the ones actually seeded and enforced —
`docs/DATABASE.md` §4 has the full catalogue including keys reserved for
later modules. `none (public)` means no `Authorization` header is required;
everything else requires a valid JWT bearer access token.

### Auth (`/api/v1/auth`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| POST | `/auth/login` | none (public) | rate-limited per IP (`RateLimiting:Auth`); records `login_history`; locks the account after `AccountSecurity:MaxFailedLoginAttempts` |
| POST | `/auth/refresh` | none (holds refresh token) | rotates the refresh token; reusing an already-rotated token revokes every active token for that user |
| POST | `/auth/logout` | authenticated | revokes exactly the presented refresh token, only if it belongs to the caller |
| POST | `/auth/forgot-password` | none (public) | rate-limited; always returns the same response shape whether or not the email exists |
| POST | `/auth/reset-password` | none (holds reset token) | single-use, time-limited token; revokes every active session on success |
| POST | `/auth/change-password` | authenticated | requires current password |
| GET | `/auth/me` | authenticated | current user profile + effective roles/permissions |

### Users (`/api/v1/users`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/users` | `users.read` | paginated, tenant-scoped by the query filter (not by this endpoint's own logic) |
| GET | `/users/{id}` | `users.read` | a user outside the caller's tenant is a 404, not a 403 |
| POST | `/users` | `users.create` | always creates a tenant-bound user; rejects assigning the Super Administrator role |
| PUT | `/users/{id}` | `users.update` | full name / phone number only |
| POST | `/users/{id}/activate` | `users.disable` | |
| POST | `/users/{id}/deactivate` | `users.disable` | also revokes every active refresh token for that user |
| POST | `/users/{id}/roles/{roleId}` | `roles.manage` | assigns an existing role; Super Administrator role requires the caller to already be one |
| DELETE | `/users/{id}/roles/{roleId}` | `roles.manage` | revokes a role assignment |

### Roles (`/api/v1/roles`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/roles` | `roles.read` | the 10 fixed system roles, each with its permission set |
| POST | `/roles/{roleId}/permissions/{permissionId}` | `roles.manage` **and** Super Administrator | roles are global (shared across every tenant) — the handler itself requires Super Administrator regardless of who else holds `roles.manage`, since that permission also covers the much narrower, org-scoped act of assigning a role to a user |
| DELETE | `/roles/{roleId}/permissions/{permissionId}` | `roles.manage` **and** Super Administrator | |

### Permissions (`/api/v1/permissions`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/permissions` | `roles.read` | the full catalogue, for building role-assignment UI |

### Organisations (`/api/v1/organisations`)

A single organisation's own profile is readable/writable by anyone holding
`organisation.read`/`organisation.write` **for their own tenant** — there
is no query filter backing that boundary (Organisation *is* the tenant, so
each handler checks explicitly; see `docs/ARCHITECTURE.md` §13). Cross-tenant
listing, creation, and deletion are Super Administrator only.

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/organisations` | `organisation.read` **and** Super Administrator | cross-tenant list; paginated/searchable/filterable by status/sortable |
| POST | `/organisations` | `organisation.write` **and** Super Administrator | provisions a new tenant (no user — see `IdentityBootstrapper` for the org-less Super Administrator path) |
| GET | `/organisations/{id}` | `organisation.read` | own organisation only, else 404 |
| PUT | `/organisations/{id}` | `organisation.write` | full profile: legal name, industry, size, country, website, primary/privacy/DPO contacts |
| DELETE | `/organisations/{id}` | `organisation.write` **and** Super Administrator | soft-delete; refuses (409) if the organisation still has any active user |
| GET | `/organisations/{id}/dashboard` | `organisation.read` | summary counts (business units, departments, active users) + primary location + DPO-configured flag |
| POST | `/organisations/{id}/locations` | `organisation.write` | setting `isPrimary` unsets any other primary location for that organisation |
| PUT | `/organisations/locations/{locationId}` | `organisation.write` | tenant-scoped by `OrganisationLocation`'s own query filter |
| DELETE | `/organisations/locations/{locationId}` | `organisation.write` | soft-delete |

### Business Units (`/api/v1/business-units`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/business-units` | `businessunits.read` | paginated/searchable/filterable by `isActive`/sortable, tenant-scoped by the query filter |
| GET | `/business-units/{id}` | `businessunits.read` | another tenant's business unit is a 404 |
| POST | `/business-units` | `businessunits.create` | rejects a duplicate name within the same organisation (409) |
| PUT | `/business-units/{id}` | `businessunits.update` | |
| DELETE | `/business-units/{id}` | `businessunits.delete` | refuses (409) if it still has any department |

### Departments (`/api/v1/departments`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/departments` | `departments.read` | paginated/searchable/filterable by `businessUnitId`/`isActive`/sortable |
| GET | `/departments/{id}` | `departments.read` | |
| POST | `/departments` | `departments.create` | takes `businessUnitId`, not `organisationId` — the organisation is always derived from the (tenant-filtered) business unit |
| PUT | `/departments/{id}` | `departments.update` | |
| DELETE | `/departments/{id}` | `departments.delete` | soft-delete |

### Compliance (`/api/v1/compliance`)

All content here is **global reference data, not tenant data** — see
`docs/ARCHITECTURE.md` §14 and `docs/COMPLIANCE_CONTENT_GOVERNANCE.md`. Every
write endpoint requires `controls.manage` **and** Super Administrator
(enforced in the handler, the same pattern as Roles' global templates); read
endpoints require only `controls.read`. Every list/detail read endpoint
forces `ControlStatus.ACTIVE`-only results for a non-Super-Administrator
caller regardless of any status filter passed — see
`docs/COMPLIANCE_CONTENT_GOVERNANCE.md` §5.

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/compliance/frameworks` | `controls.read` | all frameworks with their version summaries |
| POST | `/compliance/frameworks` | `controls.manage` **and** Super Administrator | rejects a duplicate `code` (409) |
| PUT | `/compliance/frameworks/{id}` | `controls.manage` **and** Super Administrator | |
| GET | `/compliance/framework-versions/{id}` | `controls.read` | version detail including its legal references and their requirement counts |
| POST | `/compliance/frameworks/{frameworkId}/versions` | `controls.manage` **and** Super Administrator | new versions start `IsCurrent = false`, `ReviewStatus = DRAFT` |
| PUT | `/compliance/framework-versions/{id}` | `controls.manage` **and** Super Administrator | |
| POST | `/compliance/framework-versions/{id}/activate` | `controls.manage` **and** Super Administrator | sets `IsCurrent = true` and unsets it on every other version of the same framework |
| GET | `/compliance/legal-references` | `controls.read` | optional `frameworkVersionId` filter |
| POST | `/compliance/legal-references` | `controls.manage` **and** Super Administrator | `sourceCitation` is required — "every legal reference must contain its source" |
| PUT | `/compliance/legal-references/{id}` | `controls.manage` **and** Super Administrator | |
| GET | `/compliance/requirements` | `controls.read` | optional `legalReferenceId` filter; each result includes its mapped controls |
| POST | `/compliance/requirements` | `controls.manage` **and** Super Administrator | rejects a duplicate `code` (409) |
| PUT | `/compliance/requirements/{id}` | `controls.manage` **and** Super Administrator | |
| GET | `/compliance/control-categories` | `controls.read` | includes each category's control count |
| POST | `/compliance/control-categories` | `controls.manage` **and** Super Administrator | rejects a duplicate name (409) |
| PUT | `/compliance/control-categories/{id}` | `controls.manage` **and** Super Administrator | |
| GET | `/compliance/controls` | `controls.read` | paginated/searchable/filterable by `categoryId`/`riskLevel`/`status` (Super Administrator only)/`frameworkVersionId`/`hasApplicableConditions`/sortable |
| GET | `/compliance/controls/{id}` | `controls.read` | includes mapped requirements and questions (with their evidence requirements); non-`ACTIVE` is a 404 for a non-Super-Administrator |
| POST | `/compliance/controls` | `controls.manage` **and** Super Administrator | new controls start `Status = DRAFT`, `ReviewStatus = DRAFT`; rejects a duplicate `controlId` (409) |
| PUT | `/compliance/controls/{id}` | `controls.manage` **and** Super Administrator | increments the control's own `Version` counter |
| POST | `/compliance/controls/{id}/activate` | `controls.manage` **and** Super Administrator | 409 if already `ACTIVE` |
| POST | `/compliance/controls/{id}/retire` | `controls.manage` **and** Super Administrator | 409 if already `RETIRED`; never deletes the control or its questions |
| POST | `/compliance/control-mappings` | `controls.manage` **and** Super Administrator | links a control to a requirement; rejects a duplicate pair (409) |
| DELETE | `/compliance/control-mappings/{id}` | `controls.manage` **and** Super Administrator | |
| GET | `/compliance/questions` | `controls.read` | paginated/searchable/filterable by `controlId`/`questionType` |
| POST | `/compliance/questions` | `controls.manage` **and** Super Administrator | `options` required when `questionType` is `MULTIPLE_CHOICE`/`MULTI_SELECT` |
| PUT | `/compliance/questions/{id}` | `controls.manage` **and** Super Administrator | |
| DELETE | `/compliance/questions/{id}` | `controls.manage` **and** Super Administrator | soft-delete |
| GET | `/compliance/evidence-requirements` | `controls.read` | paginated/searchable/filterable by `assessmentQuestionId` |
| POST | `/compliance/evidence-requirements` | `controls.manage` **and** Super Administrator | |
| PUT | `/compliance/evidence-requirements/{id}` | `controls.manage` **and** Super Administrator | |
| DELETE | `/compliance/evidence-requirements/{id}` | `controls.manage` **and** Super Administrator | soft-delete |
| GET | `/compliance/answer-statuses` | `controls.read` | static reference list (`AnswerStatus` enum names) |

### Assessments (`/api/v1/assessments`)

Tenant-scoped entirely by `Assessment`'s own EF query filter (denormalized
`organisation_id` on every child table too — see `docs/ARCHITECTURE.md`
§15). `assessments.create` gates everything an assessor does (create,
edit, assign, save answers, submit, reopen) — matching the rest of this
API's permission-based (not ownership-based) model, an assignment is a
UI/reporting hint, not an access restriction. `assessments.review` and
`assessments.approve` are separate permissions for the reviewer and final
decision-maker steps.

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/assessments` | `assessments.read` | paginated/searchable/filterable by `status`/`frameworkVersionId`/`assignedToUserId`/sortable |
| POST | `/assessments` | `assessments.create` | snapshots every ACTIVE control mapped to the chosen framework version into the assessment; requires the caller to belong to an organisation (Super Administrator cannot create one) |
| GET | `/assessments/{id}` | `assessments.read` | full detail: scope, per-control status summary, review/approval history |
| PUT | `/assessments/{id}` | `assessments.create` | name/description/due date only; DRAFT or IN_PROGRESS only |
| POST | `/assessments/{id}/assign` | `assessments.create` | (re)assigns to a user in the same organisation, or clears the assignment; refused once APPROVED/ARCHIVED |
| GET | `/assessments/{id}/questionnaire` | `assessments.read` | every control and question in one nested structure — not paginated (bounded by the control library's size) |
| GET | `/assessments/{id}/score` | `assessments.read` | Overall/Control/Risk-Adjusted Score + Evidence/Assessment Coverage, computed live — see `docs/SCORING.md` |
| POST | `/assessments/{id}/submit` | `assessments.create` | DRAFT/IN_PROGRESS → SUBMITTED; 409 if any required question (on an applicable control) is unanswered |
| POST | `/assessments/{id}/review` | `assessments.review` | records one review pass; SUBMITTED → UNDER_REVIEW on the first call, additional calls while UNDER_REVIEW just add another round of feedback |
| POST | `/assessments/{id}/approve` | `assessments.approve` | UNDER_REVIEW → APPROVED only |
| POST | `/assessments/{id}/reject` | `assessments.approve` | UNDER_REVIEW → REJECTED only; `comments` required |
| POST | `/assessments/{id}/reopen` | `assessments.create` | REJECTED → IN_PROGRESS only; clears submission/decision markers for the fresh cycle |
| POST | `/assessments/{id}/archive` | `assessments.approve` | APPROVED → ARCHIVED only |
| PUT | `/assessments/answers/{assessmentControlQuestionId}` | `assessments.create` | "save progress"; upserts the one existing answer row; 409 if the assessment isn't DRAFT/IN_PROGRESS, or if marking PASS/PARTIAL without mandatory evidence attached |
| POST | `/assessments/answers/{assessmentControlQuestionId}/review` | `assessments.review` | reviewer annotates one answer (comment, optionally flags it NEEDS_REVIEW); only while SUBMITTED/UNDER_REVIEW |

### Findings (`/api/v1/findings`)

`findings.create` covers manual creation, editing, and the assessment-
derived creation path; `findings.assign` covers ownership and ordinary
progress transitions; `findings.close` gates the two closing-authority
actions (Close, Accept Risk) — see `docs/ARCHITECTURE.md` §16 for why
those two are split out from the general permission.

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/findings` | `findings.read` | paginated/searchable/filterable by `status`/`severity`/`ownerUserId`/`riskId`/`overdueOnly`/sortable — backs the Finding Dashboard |
| POST | `/findings` | `findings.create` | manual creation; starts `OPEN`, or `ASSIGNED` if an owner is given |
| POST | `/findings/from-assessment-control` | `findings.create` | the "Assessment → Failed/Partial Control → Finding" step; only accepts a FAIL/PARTIAL `AssessmentControl`; idempotent (409 on a second attempt against the same control) |
| GET | `/findings/{id}` | `findings.read` | full detail including linked risk and remediation tasks |
| PUT | `/findings/{id}` | `findings.create` | title/description/severity/asset reference/due date/recommendation |
| POST | `/findings/{id}/assign` | `findings.assign` | sets/changes the owner; `OPEN` → `ASSIGNED` on first assignment |
| POST | `/findings/{id}/status` | `findings.assign` | `ASSIGNED`/`IN_PROGRESS`/`PENDING_VERIFICATION`/`RESOLVED` only — validated against `FindingStatusTransitions`; 409 on an invalid transition |
| POST | `/findings/{id}/close` | `findings.close` | `RESOLVED` → `CLOSED` only |
| POST | `/findings/{id}/accept-risk` | `findings.close` | any active status → `ACCEPTED_RISK` (terminal); `comments` required |
| POST | `/findings/{id}/risk` | `risks.manage` | the "Finding → Risk" step; generates a new Risk Register entry seeded from the finding's severity and links it; refuses if already linked |

### Risks (`/api/v1/risks`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/risks` | `risks.read` | paginated/searchable/filterable by `riskLevel`/`status`/`ownerUserId`/sortable (default: highest score first) — backs the Risk Register |
| POST | `/risks` | `risks.manage` | computes `calculatedRiskLevel`/`calculatedRiskScore` via `IRiskScoringStrategy` — see `docs/RISK_METHODOLOGY.md` |
| GET | `/risks/{id}` | `risks.read` | includes every linked finding |
| PUT | `/risks/{id}` | `risks.manage` | recomputes the score from the (possibly changed) four input dimensions |

### Remediation (`/api/v1/remediation-tasks`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/remediation-tasks` | `remediation.read` | paginated/searchable/filterable by `status`/`ownerUserId`/`findingId`/`overdueOnly` — backs both the Remediation Dashboard and the Overdue Tasks page |
| POST | `/remediation-tasks` | `remediation.manage` | moves the parent finding to `IN_PROGRESS` if it was `OPEN`/`ASSIGNED` |
| GET | `/remediation-tasks/{id}` | `remediation.read` | includes evidence and the full comment thread |
| PUT | `/remediation-tasks/{id}` | `remediation.manage` | title/description/due date |
| POST | `/remediation-tasks/{id}/assign` | `remediation.manage` | sets/changes the owner |
| POST | `/remediation-tasks/{id}/status` | `remediation.manage` | `OPEN`/`IN_PROGRESS`/`PENDING_VERIFICATION` only — use verify/close for the rest |
| POST | `/remediation-tasks/{id}/evidence` | `remediation.manage` | replaces the evidence list; moves an `OPEN`/`IN_PROGRESS` task to `PENDING_VERIFICATION` automatically |
| POST | `/remediation-tasks/{id}/comments` | `remediation.manage` | appends to the append-only comment thread |
| POST | `/remediation-tasks/{id}/verify` | `remediation.manage` | `PENDING_VERIFICATION` → `VERIFIED` only; does **not** cascade to the parent Finding's status — see `docs/ARCHITECTURE.md` §16 |
| POST | `/remediation-tasks/{id}/close` | `remediation.manage` | `VERIFIED` → `CLOSED` only |

### Evidence (`/api/v1/evidence`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/evidence` | `evidence.read` | paginated/searchable/filterable by `status`/`evidenceType`/`ownerUserId`/`reviewerUserId`/`assessmentId`/`controlId`/`findingId`/`expiredOnly` — backs the Evidence Dashboard |
| POST | `/evidence` | `evidence.upload` | `multipart/form-data`; validates extension → content-type match → magic-byte signature → malware scan before storing, see `docs/EVIDENCE_STORAGE.md` §4 |
| GET | `/evidence/{id}` | `evidence.read` | full detail including version history and review history |
| PUT | `/evidence/{id}` | `evidence.upload` | metadata only (title/description/vendor & processing-activity references/owner/reviewer/expiry) — never touches file content or status |
| POST | `/evidence/{id}/versions` | `evidence.upload` | `multipart/form-data`; adds a new version and unconditionally resets status to `UPLOADED`, restarting the review cycle; 409 if the item is `ARCHIVED` |
| POST | `/evidence/{id}/submit` | `evidence.upload` | `UPLOADED` → `UNDER_REVIEW`; 409 if no reviewer is assigned yet |
| POST | `/evidence/{id}/approve` | `evidence.review` | `UNDER_REVIEW` → `APPROVED`; appends an `EvidenceReviewRecord` |
| POST | `/evidence/{id}/reject` | `evidence.review` | `UNDER_REVIEW` → `REJECTED`; `rejectionReason` required; appends an `EvidenceReviewRecord` |
| POST | `/evidence/{id}/archive` | `evidence.review` | `{APPROVED, REJECTED, EXPIRED}` → `ARCHIVED` (terminal) |
| POST | `/evidence/mark-expired` | `evidence.review` | batch-transitions the caller's organisation's overdue `APPROVED` evidence to `EXPIRED`; on-demand stand-in for a scheduler — see `docs/EVIDENCE_STORAGE.md` §6 |
| GET | `/evidence/{id}/download` | `evidence.read` | latest version; `Content-Disposition: attachment`; 409 if the latest version is a URL reference (nothing to download) |
| GET | `/evidence/{id}/versions/{versionNumber}/download` | `evidence.read` | a specific past version |
| GET | `/evidence/{id}/preview` | `evidence.read` | latest version, inline (no `Content-Disposition`); 409 unless the content type is in the small preview-safe allow-list — see `docs/EVIDENCE_STORAGE.md` §5 |
| GET | `/evidence/{id}/versions/{versionNumber}/preview` | `evidence.read` | a specific past version, same preview-safety rule |

### Data Sources (`/api/v1/data-sources`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/data-sources` | `datasources.read` | paginated/searchable/filterable by `sourceType`/`isActive` |
| POST | `/data-sources` | `datasources.manage` | connection secret (`password`) is encrypted before storing, never echoed back — see `docs/DATA_DISCOVERY.md` §2 |
| GET | `/data-sources/{id}` | `datasources.read` | never includes the secret or its ciphertext |
| PUT | `/data-sources/{id}` | `datasources.manage` | `password` optional — omit to leave the existing secret untouched (rotate-only-if-provided) |
| DELETE | `/data-sources/{id}` | `datasources.manage` | soft delete; 409 if a `PENDING`/`RUNNING` job exists against it |
| POST | `/data-sources/{id}/test-connection` | `datasources.manage` | opens a real connection (or checks the root path exists, for `FILE_SYSTEM`) and records the result on the DataSource |
| POST | `/data-sources/{id}/discovery-jobs` | `discoveryjobs.manage` | creates a `PENDING` job and enqueues it — the scan runs entirely in the background, see `docs/DATA_DISCOVERY.md` §5; 409 if the source is inactive or the organisation is at `MaxConcurrentJobsPerOrganisation` |

### Discovery Jobs (`/api/v1/discovery-jobs`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/discovery-jobs` | `discoveryjobs.read` | paginated/filterable by `dataSourceId`/`status` |
| GET | `/discovery-jobs/{id}` | `discoveryjobs.read` | full detail including every `DiscoveryResult` (per-asset snapshot) from that run |
| POST | `/discovery-jobs/{id}/cancel` | `discoveryjobs.manage` | 409 if the job is already in a terminal status (`COMPLETED`/`FAILED`/`CANCELLED`) |

### Data Assets & Elements (`/api/v1/data-assets`, `/api/v1/data-elements`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/data-assets` | `dataassets.read` | paginated/searchable/filterable by `dataSourceId`/`assetType` |
| GET | `/data-assets/{id}` | `dataassets.read` | full detail including every discovered `DataElement` |
| GET | `/data-elements` | `dataassets.read` | paginated/filterable by `dataAssetId`/`category`/`unclassifiedOnly`/`lowConfidenceOnly` — backs both the Data Elements and Classification dashboard views |
| GET | `/data-elements/classification-summary` | `dataassets.read` | per-category counts plus unclassified/human-corrected totals — backs the Classification dashboard |
| POST | `/data-elements/{id}/classification` | `classification.review` | human correction; `category: null` clears it; always sets 100% confidence and `ClassificationSource.HUMAN` — see `docs/DATA_DISCOVERY.md` §4 |

### Catalogues (`/api/v1/data-categories`, `/api/v1/it-systems`, `/api/v1/data-collection-sources`, `/api/v1/processors`, `/api/v1/recipients`, `/api/v1/retention-policies`)

All six follow the same shape — `GET /` (list, `search`/`isActive` filters, unpaginated up to 200), `POST /` (create), `GET /{id}`, `PUT /{id}` (full replace, including `isActive`), `DELETE /{id}` (soft delete; 409 if still referenced by a `DataInventoryItem` or `ProcessingActivity`). Read requires `datainventory.read`; every write requires `datainventory.manage`.

### Data Inventory (`/api/v1/data-inventory`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/data-inventory` | `datainventory.read` | paginated/searchable/filterable by `dataCategoryId`/`itSystemId`/`processorId`/`classification`/`riskLevel` |
| GET | `/data-inventory/export` | `datainventory.read` | same filters, unpaginated, `text/csv` — see `docs/DATA_INVENTORY.md` §6 |
| POST | `/data-inventory` | `datainventory.manage` | `dataCategoryId` required; every other catalogue reference optional |
| GET | `/data-inventory/{id}` | `datainventory.read` | |
| PUT | `/data-inventory/{id}` | `datainventory.manage` | |
| DELETE | `/data-inventory/{id}` | `datainventory.manage` | soft delete |

### Processing Activities (`/api/v1/processing-activities`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/processing-activities` | `processingactivities.read` | paginated/searchable/filterable by `status`/`ownerUserId`/`dataCategoryId`/`itSystemId` |
| GET | `/processing-activities/export` | `processingactivities.read` | same filters, unpaginated, `text/csv` |
| POST | `/processing-activities` | `processingactivities.manage` | starts `DRAFT`; `dataCategoryIds`/`itSystemIds`/`dataCollectionSourceIds`/`recipientIds`/`processorIds` set the many-to-many relationships wholesale |
| GET | `/processing-activities/{id}` | `processingactivities.read` | full detail including every linked catalogue entity |
| PUT | `/processing-activities/{id}` | `processingactivities.manage` | 409 if `ARCHIVED`; replaces every relationship set wholesale |
| DELETE | `/processing-activities/{id}` | `processingactivities.manage` | soft delete |
| POST | `/processing-activities/{id}/submit` | `processingactivities.manage` | `DRAFT` → `IN_REVIEW` |
| POST | `/processing-activities/{id}/approve` | `processingactivities.approve` | `IN_REVIEW` → `APPROVED` |
| POST | `/processing-activities/{id}/send-back` | `processingactivities.review` | `IN_REVIEW` → `DRAFT` only; `reviewComments` required — see `docs/DATA_INVENTORY.md` §3 |
| POST | `/processing-activities/{id}/archive` | `processingactivities.manage` | `APPROVED` → `ARCHIVED` (terminal) |
| POST | `/processing-activities/{id}/reopen` | `processingactivities.manage` | `APPROVED` → `DRAFT` only — distinct from send-back, see `docs/DATA_INVENTORY.md` §3 |

### Data Flows (`/api/v1/data-flows`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/data-flows` | `dataflows.read` | paginated/searchable/filterable by `processingActivityId`/`crossBorderOnly` |
| POST | `/data-flows` | `dataflows.manage` | `crossBorderCountry` required when `isCrossBorder` is true |
| GET | `/data-flows/{id}` | `dataflows.read` | |
| PUT | `/data-flows/{id}` | `dataflows.manage` | |
| DELETE | `/data-flows/{id}` | `dataflows.manage` | soft delete |

### Data Principals (`/api/v1/data-principals`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/data-principals` | `dataprincipals.read` | `search`/`isActive` filters, unpaginated up to 200 |
| POST | `/data-principals` | `dataprincipals.manage` | `externalReferenceId` required; no name/email/phone field exists — see `docs/CONSENT_PRIVACY_OPERATIONS.md` §1 |
| GET | `/data-principals/{id}` | `dataprincipals.read` | |
| PUT | `/data-principals/{id}` | `dataprincipals.manage` | |
| DELETE | `/data-principals/{id}` | `dataprincipals.manage` | soft delete; 409 if still referenced by a `ConsentRecord` or `DataPrincipalRequest` |

### Consent Purposes (`/api/v1/consent-purposes`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/consent-purposes` | `consentpurposes.read` | `search`/`isActive` filters, unpaginated up to 200 |
| POST | `/consent-purposes` | `consentpurposes.manage` | optional `dataCategoryId` link into Module 9's catalogue |
| GET | `/consent-purposes/{id}` | `consentpurposes.read` | |
| PUT | `/consent-purposes/{id}` | `consentpurposes.manage` | |
| DELETE | `/consent-purposes/{id}` | `consentpurposes.manage` | soft delete; 409 if still referenced by a `ConsentRecord` |

### SLA Policies (`/api/v1/sla-policies`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/sla-policies` | `datarequests.read` | `search`/`isActive` filters, unpaginated up to 200 |
| POST | `/sla-policies` | `datarequests.manage` | `requestType` nullable (catch-all); 409 if an active policy already covers that type — see `docs/CONSENT_PRIVACY_OPERATIONS.md` §5 |
| GET | `/sla-policies/{id}` | `datarequests.read` | |
| PUT | `/sla-policies/{id}` | `datarequests.manage` | |
| DELETE | `/sla-policies/{id}` | `datarequests.manage` | soft delete |

### Privacy Notices (`/api/v1/privacy-notices`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/privacy-notices` | `privacynotices.read` | paginated/searchable/filterable by `status`/`code` |
| POST | `/privacy-notices` | `privacynotices.manage` | starts `DRAFT`; 409 if `(code, version)` already exists |
| GET | `/privacy-notices/{id}` | `privacynotices.read` | includes linked `dataCategories` |
| PUT | `/privacy-notices/{id}` | `privacynotices.manage` | 409 unless `DRAFT` — see `docs/CONSENT_PRIVACY_OPERATIONS.md` §2 |
| DELETE | `/privacy-notices/{id}` | `privacynotices.manage` | 409 unless `DRAFT` |
| POST | `/privacy-notices/{id}/approve` | `privacynotices.approve` | `DRAFT` → `APPROVED` |
| POST | `/privacy-notices/{id}/publish` | `privacynotices.manage` | `APPROVED` → `PUBLISHED` |
| POST | `/privacy-notices/{id}/archive` | `privacynotices.manage` | `PUBLISHED` → `ARCHIVED` (terminal) |

### Consent Records (`/api/v1/consent-records`)

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/consent-records` | `consent.read` | paginated/filterable by `status`/`dataPrincipalId`/`consentPurposeId` |
| POST | `/consent-records` | `consent.manage` | starts `GRANTED`; no `PUT`/`DELETE` — see `docs/CONSENT_PRIVACY_OPERATIONS.md` §3 |
| GET | `/consent-records/{id}` | `consent.read` | |
| POST | `/consent-records/{id}/withdraw` | `consent.manage` | data-principal-initiated; `GRANTED` → `WITHDRAWN` |
| POST | `/consent-records/{id}/revoke` | `consent.manage` | organisation-initiated; `reason` required; `GRANTED` → `REVOKED` |
| POST | `/consent-records/mark-expired` | `consent.manage` | on-demand batch job stand-in; transitions past-`expiresAt` `GRANTED` records to `EXPIRED` |

### Data Principal Requests (`/api/v1/data-principal-requests`)

Covers both Data Principal Requests and Grievances — `requestType:
GRIEVANCE` uses the identical shape and workflow. See
`docs/CONSENT_PRIVACY_OPERATIONS.md` §4.

| Method | Path | Permission | Notes |
|---|---|---|---|
| GET | `/data-principal-requests` | `datarequests.read` | paginated/filterable by `status`/`requestType`/`assignedToUserId`/`overdueOnly` |
| GET | `/data-principal-requests/sla-summary` | `datarequests.read` | open/overdue/due-within-48h/no-SLA counts, computed on demand |
| POST | `/data-principal-requests` | `datarequests.manage` | starts `REQUESTED`; `dueAt` computed from the matching `SlaPolicy`, `null` if none configured — see `docs/CONSENT_PRIVACY_OPERATIONS.md` §5 |
| GET | `/data-principal-requests/{id}` | `datarequests.read` | |
| DELETE | `/data-principal-requests/{id}` | `datarequests.manage` | soft delete |
| POST | `/data-principal-requests/{id}/assign` | `datarequests.manage` | sets `assignedToUserId` |
| POST | `/data-principal-requests/{id}/verify-identity` | `datarequests.manage` | `REQUESTED` → `IDENTITY_VERIFICATION`; optional `matchedDataPrincipalId` |
| POST | `/data-principal-requests/{id}/status` | `datarequests.manage` | `IN_PROGRESS` ⇄ `AWAITING_INFORMATION` only — use the dedicated actions below for closing transitions |
| POST | `/data-principal-requests/{id}/complete` | `datarequests.manage` | → `COMPLETED`; `resolutionNotes` optional |
| POST | `/data-principal-requests/{id}/reject` | `datarequests.manage` | → `REJECTED`; `rejectionReason` required |
| POST | `/data-principal-requests/{id}/close` | `datarequests.manage` | `COMPLETED`/`REJECTED` → `CLOSED` (terminal) |

### Not yet implemented

- `GET /api/v1/audit-logs` — write path exists and is exercised by every
  mutating endpoint above; no read endpoint yet.
- Custom (non-system) role creation.
- Real notification delivery (email/Teams) — `INotificationService` is
  called from finding/remediation assignment today, but its only
  implementation logs the message; see `docs/ARCHITECTURE.md` §16.
- Asset Inventory — `Finding.AssetReference` is a free-text placeholder
  until that module (roadmap item 16) ships a real `Asset` entity.
- Vendor Management / Processing Activities — `EvidenceItem.VendorReference`/
  `.ProcessingActivityReference` are free-text placeholders until those
  modules (roadmap items 28 and 20) ship real entities.
- A public, unauthenticated Data Principal portal — this module's domain
  model is portal-ready (see `docs/CONSENT_PRIVACY_OPERATIONS.md` §6),
  but every endpoint above still requires internal authentication; a
  portal's own auth/rate-limiting/CAPTCHA model is future scope.
- Real malware scanning — `IMalwareScanner`'s only implementation is a
  no-op that always reports clean; see `docs/EVIDENCE_STORAGE.md` §3.
- A recurring scheduler — `POST /evidence/mark-expired` is an on-demand
  stand-in until one exists; see `docs/EVIDENCE_STORAGE.md` §6.
- A durable/distributed discovery job queue — `IDiscoveryJobQueue`'s only
  implementation is a single-process, in-memory channel; see
  `docs/DATA_DISCOVERY.md` §5.
- Recurring/scheduled discovery scans — every scan is started on demand;
  no cron-style trigger exists yet.
- Live MySQL/SQL Server connector integration testing — no such server
  exists in this environment; see `docs/DATA_DISCOVERY.md` §8.

### Health

| Method | Path | Notes |
|---|---|---|
| GET | `/health` | liveness, no dependency checks |
| GET | `/health/ready` | readiness, checks PostgreSQL connectivity (a Redis check is added once a module actually uses Redis) |

## 3. Contract Stability

DTOs for the above are defined once in `DPDP.Application/Modules/<Module>/DTOs`
and are the single source of truth; the frontend's Zod schemas mirror them
for client-side UX only (see `ARCHITECTURE.md` §2 rule against sharing
business logic) and must be kept in sync by hand at each change — there is
no code-generation step in Phase 1 (would be a reasonable Phase 3+
addition, e.g. `openapi-typescript`, but is not required to ship Phase 1
correctly and is deferred per the no-overengineering principle).
