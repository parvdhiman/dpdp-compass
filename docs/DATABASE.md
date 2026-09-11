# DPDP-COMPASS — Database

## 1. Conventions (apply to every table, every phase)

- **Engine:** PostgreSQL.
- **Naming:** `snake_case` for tables and columns; table names plural
  (`organisations`, `role_permissions`).
- **Primary keys:** `id uuid` — generated application-side (`Guid.CreateVersion7()`
  where available, else `Guid.NewGuid()`), not a DB default, so IDs are
  known before `SaveChanges` (needed for correlating audit logs and
  cross-aggregate references within one unit of work).
- **Tenant column:** every tenant-scoped table carries `organisation_id uuid
  not null references organisations(id)` — this is the `tenant_id` referred
  to generically in `ARCHITECTURE.md` §4. Tables that are inherently
  global (e.g. `permissions`, the static catalogue; `frameworks`, versioned
  law content) do **not** carry it.
- **Audit columns:** `created_at timestamptz not null`, `created_by uuid
  null references users(id)`, `updated_at timestamptz not null`,
  `updated_by uuid null references users(id)`. All timestamps UTC.
- **Soft delete:** `is_deleted boolean not null default false`,
  `deleted_at timestamptz null`, `deleted_by uuid null` — applied only to
  entities where a hard delete would break referential/audit integrity
  (e.g. `users`, `organisations`, `findings`); pure lookup/link tables use
  hard delete.
- **Optimistic concurrency:** `row_version` (Postgres `xmin` mapped via EF
  Core's built-in concurrency token, no extra column needed) on entities
  editable by multiple actors concurrently (e.g. `assessments`, `findings`
  from Phase 2) — not applied to Phase 1's append-mostly tables.
- **Constraints:** every FK declared; `not null` unless a column is
  genuinely optional (no "nullable by default" columns); `unique`
  constraints scoped by tenant where relevant (e.g. `unique
  (organisation_id, email)` on `users`, not a bare global unique on
  `email`, since two different customer organisations may legitimately
  share no data but could — in theory — have same-named contacts; actual
  login identifier is `email` **and** the account is tenant-bound, so the
  practical uniqueness needed is per-tenant. Super Administrator accounts,
  which have `organisation_id is null`, are constrained unique on `email`
  globally via a partial unique index).
- **No duplicated data**, no unbounded `text` where a bounded `varchar` is
  correct, no compliance logic encoded in triggers/stored procedures —
  logic lives in the Application layer per `ARCHITECTURE.md`.

## 2. Migration Strategy

One `DpdpDbContext` (in `DPDP.Infrastructure`), configured via one
`IEntityTypeConfiguration<T>` class per entity, physically grouped under
`Persistence/Configurations/<Module>/`. EF Core Code-First migrations, one
migrations project, migration names prefixed by module
(`20260907_Identity_Initial`, `20260907_Organisations_Initial`, ...) even
though they land in a single `Migrations/` folder — this keeps a modular
monolith's single physical database auditable per module without needing
per-module databases (which §23 would call overengineering for this
project's actual scale).

## 3. Phase 1 Schema

```
organisations
  id                  uuid PK
  name                varchar(200) not null
  legal_name          varchar(300) null
  status              varchar(20) not null   -- active | suspended
  created_at, updated_at, created_by, updated_by, is_deleted, deleted_at, deleted_by

users
  id                  uuid PK
  organisation_id     uuid null references organisations(id)   -- null only for Super Administrator
  email               varchar(320) not null
  normalized_email    varchar(320) not null
  password_hash       varchar(500) not null
  security_stamp      varchar(100) not null
  full_name           varchar(200) not null
  phone_number        varchar(30) null
  is_active           boolean not null default true
  mfa_enabled         boolean not null default false      -- MFA-ready, not enforced Phase 1
  mfa_secret          varchar(200) null                    -- encrypted at rest
  failed_login_count  int not null default 0
  lockout_end         timestamptz null
  last_login_at       timestamptz null
  created_at, updated_at, created_by, updated_by, is_deleted, deleted_at, deleted_by
  constraints:
    unique (organisation_id, normalized_email) where organisation_id is not null
    unique (normalized_email) where organisation_id is null   -- Super Administrator

refresh_tokens
  id                  uuid PK
  user_id             uuid not null references users(id)
  token_hash          varchar(500) not null      -- token itself never stored
  expires_at          timestamptz not null
  revoked_at          timestamptz null
  replaced_by_token_id uuid null references refresh_tokens(id)
  created_at          timestamptz not null
  created_by_ip       varchar(64) null

login_history
  id                  uuid PK
  user_id             uuid null references users(id)   -- null if the email didn't match any account
  organisation_id     uuid null references organisations(id)
  attempted_email     varchar(320) not null
  succeeded           boolean not null
  failure_reason      varchar(100) null
  ip_address          varchar(64) null
  user_agent          varchar(300) null
  created_at          timestamptz not null

roles
  id                  uuid PK
  name                varchar(100) not null unique   -- e.g. "Organisation Administrator"
  description         varchar(500) null
  is_system_role      boolean not null default true   -- seeded roles vs. (future) custom roles
  created_at, updated_at

permissions
  id                  uuid PK
  key                 varchar(150) not null unique   -- e.g. "assessment.approve"
  description         varchar(300) not null
  module              varchar(100) not null           -- e.g. "Assessments"
  created_at

role_permissions
  role_id             uuid not null references roles(id)
  permission_id       uuid not null references permissions(id)
  PK (role_id, permission_id)

user_roles
  user_id             uuid not null references users(id)
  role_id             uuid not null references roles(id)
  organisation_id     uuid null references organisations(id)   -- scope of this assignment; null = global (Super Admin only)
  assigned_at         timestamptz not null
  assigned_by         uuid null references users(id)
  PK (user_id, role_id, organisation_id)

audit_logs
  id                  uuid PK
  organisation_id     uuid null references organisations(id)
  user_id             uuid null references users(id)
  action              varchar(150) not null        -- e.g. "user.role_changed"
  entity_type         varchar(150) not null
  entity_id           varchar(150) null
  old_value           jsonb null
  new_value           jsonb null
  ip_address          varchar(64) null
  user_agent          varchar(300) null
  correlation_id      varchar(64) null
  created_at          timestamptz not null
  -- append-only: no updated_at/updated_by/is_deleted; see SECURITY.md for tamper-resistance
```

### Indexing (Phase 1)

- `users`: partial unique indexes above; index on `organisation_id`; index
  on `normalized_email` for login lookup.
- `refresh_tokens`: index on `user_id`; index on `token_hash`.
- `login_history`: index on `(organisation_id, created_at)` for lockout/
  reporting queries; index on `user_id`.
- `user_roles`: index on `(user_id, organisation_id)`.
- `audit_logs`: index on `(organisation_id, created_at)`; index on
  `(entity_type, entity_id)`; index on `correlation_id`.

### Relationships (Phase 1 ERD, text form)

```
organisations 1───* users
users 1───* refresh_tokens
users 1───* login_history (nullable FK, since a failed login on an unknown email has no user)
organisations 1───* login_history (nullable)
roles *───* permissions   (via role_permissions)
users *───* roles         (via user_roles, scoped by organisation_id)
organisations 1───* audit_logs (nullable, Super Admin actions may have null org)
users 1───* audit_logs (nullable, system-originated events may have null user)
```

## 4. Seed Data (Phase 1)

- **Permissions:** the full catalogue from MASTER_PROMPT §6
  (`organisation.read`, `organisation.write`, `assessment.read`,
  `assessment.create`, `assessment.approve`, `control.read`,
  `control.manage`, `evidence.upload`, `evidence.review`, `finding.create`,
  `finding.assign`, `finding.close`, `risk.manage`, `report.generate`,
  `user.manage`, `audit.read`, plus the Phase-1-relevant subset actually
  enforceable now: `organisation.read/write`, `user.manage`, `audit.read`,
  `role.manage`). Permissions for not-yet-built modules are seeded now so
  role templates are complete and stable; enforcement of those permissions
  activates only once the corresponding module ships.
- **Roles:** the ten roles from MASTER_PROMPT §6, each with a starting
  `role_permissions` set (e.g. Auditor gets `*.read` + `audit.read`, never
  a `*.write`; Read Only User gets `*.read` only; Super Administrator gets
  everything including cross-tenant `organisation.write`).
- **First organisation + first Super Administrator user:** created via a
  one-time seed/bootstrap script (`scripts/seed.sh`), never hard-coded
  credentials in source — the bootstrap prompts for or reads the initial
  admin email/password from environment variables at first run.

## 5. Module 2 (Identity, Authentication & RBAC) — As Implemented

- **Naming convention enforcement:** `EFCore.NamingConventions`
  (`UseSnakeCaseNamingConvention()`) is applied to the whole `DpdpDbContext`
  rather than hand-specifying every column name — it converts
  `OrganisationId` → `organisation_id` etc. automatically, so §1's
  snake_case rule holds without per-property configuration. Applied in
  both the runtime `AddInfrastructure` registration and the design-time
  factory used by `dotnet ef`; they must stay in sync or migrations drift.
- **`is_deleted` has no database default** — it's a plain
  `not null` boolean with the application always setting it explicitly
  (via `AuditableEntity`/`ISoftDeletable` object initialization). A raw
  `INSERT` that omits it will fail — this is intentional, not a bug: it
  surfaced a manual test `INSERT` immediately rather than silently leaving
  a row in an ambiguous state.
- **`user_roles` uses a surrogate `id`, not a composite primary key** — see
  `UserRole.cs`'s doc comment: PostgreSQL primary key columns can't be
  nullable, and `organisation_id` is null for a Super Administrator's
  global role assignment. Uniqueness is instead two partial unique indexes
  (`WHERE organisation_id IS NOT NULL` / `WHERE organisation_id IS NULL`).
  The same pattern is used for `users`' per-tenant email uniqueness.
- **`password_reset_tokens` table added** (not in the original Phase 1
  sketch): same shape as `refresh_tokens` — only a SHA-256 hash is stored,
  single-use (`used_at`), time-limited.
- **Static reference data (`permissions`, `roles`, `role_permissions`) is
  seeded via EF Core migration `HasData`**, not a runtime seeder — it's
  genuinely static, so it belongs in the migration itself, not application
  startup code. IDs are deterministic (`DeterministicGuid.Create("permission:users.read")`,
  an MD5-based pure function — not cryptographic, only used so the seed
  can be expressed as stable strings instead of a hand-maintained list of
  random GUID literals) so the same migration always produces the same
  rows. The **first organisation + Super Administrator**, by contrast, is
  genuinely dynamic (needs a real password hash from operator-supplied
  credentials) and is created by `IdentityBootstrapper` at application
  startup instead — idempotent, skipped once any Super Administrator
  exists. See docs/DEVELOPMENT.md.
- **Actual permission catalogue** (supersedes this document's earlier
  illustrative list): `organisation.read/write`, `users.read/create/update/disable`,
  `roles.read/manage`, plus `assessments.*`, `controls.*`, `risks.*`,
  `findings.*`, `remediation.*`, `evidence.*`, `reports.*`, `audit.read` —
  seeded now, enforced starting whichever later module owns them. See
  `PermissionKeys` in `DPDP.Domain.Modules.Identity` for the exact keys and
  `RolePermissionConfiguration` for the default role → permission mapping.

## 6. Module 3 (Organisation Management) — As Implemented

New tables: `organisation_locations`, `business_units`, `departments`.
`organisations` gains `industry`, `size` (nullable enum, `HasConversion<string>`),
`country`, `website`, and nine contact columns
(`primary_contact_{name,email,phone}`, `privacy_contact_{name,email,phone}`,
`dpo_{name,email,phone}`) via three EF Core owned-type mappings of one
shared `ContactInfo` value object — see `docs/ARCHITECTURE.md` §13.

- **`business_units.organisation_id`, `departments.organisation_id`,
  `departments.business_unit_id`, and `organisation_locations.organisation_id`
  are all `not null`** — these entities always belong to exactly one
  parent, unlike `users.organisation_id` (nullable, for Super
  Administrator). They implement `ITenantScoped` directly and get the
  standard `!is_deleted && (super_admin || organisation_id = :current)`
  filter via `DpdpDbContext.ApplyTenantScopedFilter<TEntity>()` — no
  per-entity filter lambda needed.
- **`business_units.organisation_id → organisations.id` and
  `departments.business_unit_id → business_units.id` are `ON DELETE
  RESTRICT`, not `CASCADE`** — deliberately: the application layer decides
  whether a delete is safe (refusing if children exist, per
  `docs/ARCHITECTURE.md` §13's "conservative deletes" note), and a DB-level
  cascade would silently bypass that decision. This also means a test
  fixture that soft-deletes a BusinessUnit/Department via the API must
  still hard-delete it directly before hard-deleting its parent
  Organisation in teardown — see the Module 3 completion report's Known
  Issues for the test-hygiene bug this caused.
- **No new permission naming pattern** — `businessunits.{read,create,update,delete}`
  and `departments.{read,create,update,delete}` extend the same
  `module.action` scheme from Module 2, just added to `PermissionKeys` and
  seeded via the same `HasData`-in-migration mechanism (new deterministic
  IDs, existing rows untouched).

## 7. Module 4 (DPDP Compliance Framework & Control Library) — As Implemented

New tables: `compliance_frameworks`, `compliance_framework_versions`,
`legal_references`, `requirements`, `control_categories`, `controls`,
`control_mappings`, `assessment_questions`, `evidence_requirements`. See
`docs/ARCHITECTURE.md` §14 and `docs/COMPLIANCE_CONTENT_GOVERNANCE.md` for
the full design rationale; this section covers only the schema-level
facts.

- **None of these tables carry `organisation_id` or implement
  `ITenantScoped`.** They are global reference data (one shared,
  versioned legal-content library every tenant reads from), the first
  tables in the schema to be deliberately excluded from tenant
  partitioning. Only `assessment_questions` and `evidence_requirements`
  (the two entities with a delete action) get a soft-delete query filter;
  every other table in this module has no query filter at all.
- **`control_mappings` is a pure many-to-many join table** between
  `controls` and `requirements` (`ControlId`, `RequirementId`,
  `MappingNotes`), not a foreign key on either side — a control can
  satisfy more than one requirement, and a requirement can be satisfied by
  more than one control, over time.
- **`controls.status` (`DRAFT`/`ACTIVE`/`RETIRED`) and
  `*.review_status` (`DRAFT`/`LEGAL_REVIEWED`/`APPROVED`) are two separate
  `HasConversion<string>` enum columns**, never conflated — see
  `docs/COMPLIANCE_CONTENT_GOVERNANCE.md` §4 for what each promises.
  `review_status` also exists on `compliance_framework_versions`,
  `legal_references`, and `requirements` — anywhere legal-content wording
  can be edited.
- **`assessment_questions.options_json` is a `jsonb` column** holding a
  JSON array of option strings (used only when `question_type` is
  `MULTIPLE_CHOICE` or `MULTI_SELECT`), serialized/deserialized in the
  Application layer — no separate options table.
- **`framework_versions.effective_date` and `.publication_date` are both
  nullable `date` columns** (`DateOnly?` in C#) — an Act can be assented
  to and published before its obligations take legal effect (staggered
  commencement), so `effective_date = NULL` is a valid, expected state,
  not missing data. See `docs/COMPLIANCE_CONTENT_GOVERNANCE.md` §6.
- **All seed rows are inserted via `HasData` in the
  `ComplianceFrameworkAndControls` migration**, sourced from the single
  reviewable `DpdpActSeedData.cs` file (`docs/COMPLIANCE_CONTENT_GOVERNANCE.md`
  §8), using the same `DeterministicGuid.Create(...)`-derived id pattern
  as every other seeded table in this project — seeded: 2 frameworks, 2
  framework versions, 11 legal references, 13 requirements, 8 control
  categories, 13 controls, 13 control mappings, 13 assessment questions,
  13 evidence requirements.
- **No new permission naming pattern** — reuses `controls.read` and
  `controls.manage`, both already seeded in Module 2's `PermissionKeys` in
  anticipation of this module (see `docs/ARCHITECTURE.md` §12).

## 8. Module 5 (DPDP Compliance Assessment Engine) — As Implemented

New tables: `assessments`, `assessment_scopes`, `assessment_controls`,
`assessment_control_questions`, `assessment_answers`, `assessment_reviews`,
`assessment_approvals`. See `docs/ARCHITECTURE.md` §15 and
`docs/SCORING.md` for design rationale; this section covers schema facts.

- **Every table here carries `organisation_id` and is `ITenantScoped`** —
  unlike Module 4's Compliance tables, this is genuinely tenant-owned
  data. `organisation_id` is denormalized onto every child table (copied
  from the parent `Assessment` at creation), not left to a join, matching
  the `Department.organisation_id` precedent from Module 3 (§6). Every FK
  column also gets an index (`assessment_id`, `control_id`,
  `assessment_control_id`, `assessment_control_question_id`,
  `assigned_to_user_id`, `reviewer_id`, `decided_by_user_id`), plus a
  composite `(organisation_id, status)` index on `assessments` for the
  list/filter endpoint.
- **`assessment_control_questions.(assessment_control_id, question_id)`
  and `assessment_controls.(assessment_id, control_id)` are unique
  composite indexes** — a control or question can appear at most once per
  assessment, enforced at the database level, not just by
  `CreateAssessmentCommand`'s logic.
- **`assessment_answers.assessment_control_question_id` is a unique FK** —
  the 1:1 relationship between a question-in-an-assessment and its answer
  is enforced by the schema, not just convention.
- **Two enum columns on `assessment_controls`/`assessment_answers` reuse
  Module 4's `AnswerStatus`** (`HasConversion<string>`, same as every
  other enum column in this project) rather than a duplicate near-identical
  enum — see `docs/ARCHITECTURE.md` §15.
- **`assessment_answers.answer_value` / `.answer_values_json` /
  `.evidence_json`**: scalar answers (YES_NO/TEXT/NUMBER/DATE/URL/FILE)
  use the plain string column; `MULTI_SELECT` answers and the evidence
  reference list both use `jsonb` columns, the same pattern Module 4
  established for `AssessmentQuestion.options_json`.
- **Delete behavior**: every child table cascades from `assessments`
  (`ON DELETE CASCADE` on `assessment_id`/`assessment_control_id`/
  `assessment_control_question_id` FKs) — deliberately, since an
  Assessment's children have no independent lifecycle of their own (unlike
  Module 3's conservative-delete Organisation/BusinessUnit tree). FKs
  pointing *into* the global Compliance library (`control_id`,
  `question_id` via `assessment_control_questions`) are `ON DELETE
  RESTRICT`, matching that library's own conservative-delete convention.
  This cascade is also what makes test cleanup simple — deleting the
  `assessments` row for a test organisation cascades every row below it in
  one statement (see `AssessmentApiFixture.DisposeAsync`).
- **One new permission**: `assessments.review`, added alongside the
  already-seeded `assessments.read`/`assessments.create`/`assessments.approve`
  from Module 2's forward-looking catalogue (`docs/DATABASE.md` §5) —
  granted to the Compliance Officer role in the same migration.
- **No new tables for evidence or scoring** — evidence is a `jsonb` column
  on `assessment_answers` (§ above), and scores are always computed live,
  never persisted (`docs/SCORING.md` §1).

## 9. Module 6 (Findings, Risk & Remediation) — As Implemented

New tables: `findings`, `risks`, `remediation_tasks`, `remediation_comments`.
See `docs/ARCHITECTURE.md` §16 and `docs/RISK_METHODOLOGY.md` for design
rationale; this section covers schema facts.

- **`findings.sequence_number` and `risks.sequence_number` are Postgres
  identity columns** (`UseIdentityColumn()`), not application-generated
  counters — atomic under concurrent creation, no race condition. The
  human-facing `FIND-00001`/`RISK-00001` display value is computed from
  this at the Application layer, never stored as a formatted string. Each
  is covered by a unique `(organisation_id, sequence_number)` index.
- **`findings.asset_reference` is a plain nullable string column, not a
  foreign key** — Asset Inventory (roadmap item 16) doesn't exist yet;
  see `docs/ARCHITECTURE.md` §16 for the migration path once it does.
- **`findings.assessment_id`/`.assessment_control_id`/`.control_id`/
  `.risk_id`/`.owner_user_id` are all `ON DELETE SET NULL`** — a finding
  should never disappear because the assessment, control, risk, or user it
  referenced was later removed; it just loses that particular linkage.
  `remediation_tasks.finding_id`, by contrast, is `ON DELETE CASCADE` —
  a remediation task has no independent lifecycle apart from its finding
  (the same reasoning Module 5 applied to Assessment's own children).
  `remediation_comments.remediation_task_id` cascades the same way.
- **Two enum columns reuse existing enums rather than adding near-
  duplicates**: `risks.data_sensitivity`/`.exposure`/`.calculated_risk_level`
  all reuse Module 4's `Compliance.RiskLevel` (LOW/MEDIUM/HIGH/CRITICAL);
  `findings.severity` gets its own `FindingSeverity` enum only because it
  has a fifth value (`INFORMATIONAL`) `RiskLevel` doesn't.
- **`remediation_tasks.evidence_json` is a `jsonb` column**, the same
  lightweight evidence-reference pattern as Module 5's
  `assessment_answers.evidence_json` — no file upload/storage yet.
- **Indexes**: every FK column gets one; `findings` additionally indexes
  `(organisation_id, status)` and `(organisation_id, severity)`,
  `risks` indexes `(organisation_id, calculated_risk_level)` and
  `(organisation_id, status)`, and `remediation_tasks` indexes `due_date`
  directly — all three back the dashboard/filter queries the frontend
  actually issues (Finding Dashboard, Risk Register, Remediation
  Dashboard, Overdue Tasks).
- **One role-permission fix, in an additive migration**: `findings.create`
  was added to the Compliance Officer and Security Officer roles (only
  Privacy Officer had it before this module) — see
  `docs/ARCHITECTURE.md` §16 for why this was a real gap, not a stylistic
  choice. No new permission keys were needed otherwise — every key this
  module enforces (`findings.*`, `risks.*`, `remediation.*`) was already
  seeded in Module 2's forward-looking catalogue (§5 above).

## 10. Module 7 (Evidence Management) — As Implemented

New tables: `evidence_items`, `evidence_versions`, `evidence_review_records`.
See `docs/ARCHITECTURE.md` §17 and `docs/EVIDENCE_STORAGE.md` for design
rationale; this section covers schema facts. Pre-existing Module 4 table
`evidence_requirements` is unrelated (a requirement *definition* on a
Control, not an uploaded evidence *item*) — confirmed no naming or FK
collision when these migrations were added.

- **`evidence_items.sequence_number` is a Postgres identity column**
  (`UseIdentityColumn()`), the same pattern as `findings`/`risks` —
  atomic under concurrent creation, human-facing `EVID-00001` computed at
  the Application layer. Covered by a unique
  `(organisation_id, sequence_number)` index.
- **No file content column anywhere.** `evidence_versions` stores only
  `storage_key`, `original_file_name`, `content_type`, `size_bytes`,
  `checksum_sha256`, `external_url` (mutually exclusive with the file
  columns — URL-type evidence has no storage key), and
  `malware_scan_status`/`malware_scan_details`. The actual bytes live
  behind `IObjectStorageService`, addressed by `storage_key` — see
  `docs/EVIDENCE_STORAGE.md` §1–2.
- **`evidence_items.assessment_id`/`.control_id`/`.finding_id` are all
  `ON DELETE SET NULL`** — the same reasoning as `findings`' equivalent
  columns (§9 above): evidence should never disappear because the
  assessment/control/finding it was attached to was later removed.
- **`evidence_items.vendor_reference`/`.processing_activity_reference`
  are plain nullable string columns, not foreign keys** — Vendor
  Management (roadmap item 28) and Processing Activities (roadmap item
  20) don't exist yet; see `docs/ARCHITECTURE.md` §17 for the migration
  path once they do.
- **`evidence_versions.metadata_json` is a `jsonb` column** for
  arbitrary per-version metadata that doesn't warrant its own column
  (e.g. tool-specific export metadata) — a lighter version of the
  `evidence_json` pattern Modules 5/6 used, here a sidecar on the version
  row rather than the primary carrier of evidence data (this module's
  evidence *is* the primary entity, not a JSON attachment on something
  else).
- **`evidence_versions.(evidence_item_id, version_number)` is unique**,
  and `evidence_review_records.evidence_version_number` is a plain `int`
  (not a FK to `evidence_versions`) — a review record intentionally
  survives even if the specific version it reviewed is superseded by a
  later upload; it records *which version number* was reviewed at the
  time, not a live reference to a row that might later represent a
  different version's history.
- **Cascade behavior**: `evidence_versions.evidence_item_id` and
  `evidence_review_records.evidence_item_id` both `ON DELETE CASCADE` —
  neither has an independent lifecycle apart from its parent
  `EvidenceItem`, the same reasoning Module 6 applied to
  `remediation_comments`.
- **Indexes**: every FK column gets one; `evidence_items` additionally
  indexes `(organisation_id, status)`, `(organisation_id, evidence_type)`,
  and `expiry_date` — backing the Evidence Dashboard's status/type filters
  and the `MarkEvidenceExpiredCommand` batch query.
- **No new permission keys were needed** — `evidence.read`,
  `evidence.upload`, and `evidence.review` were already seeded in Module
  2's forward-looking catalogue and already assigned to their intended
  roles (§5 above); this module's contribution was implementing the
  enforcement, not the permission catalogue.

## 11. Module 8 (Data Discovery Engine) — As Implemented

New tables: `data_sources`, `discovery_jobs`, `discovery_results`,
`data_assets`, `data_elements`. See `docs/ARCHITECTURE.md` §18 and
`docs/DATA_DISCOVERY.md` for design rationale; this section covers schema
facts.

- **No file/row content column anywhere in this module.**
  `data_sources.encrypted_secret` stores ciphertext (ASP.NET Core Data
  Protection output), never a plaintext credential.
  `data_elements.sample_masked_value` stores only an already-masked
  string (max 300 chars) — there is no raw-sample column, by design; see
  `docs/DATA_DISCOVERY.md` section 3.
- **`data_assets` is unique on `(data_source_id, schema_name, asset_name)`**
  — a second discovery run against the same table updates the existing
  row rather than inserting a duplicate. `data_elements` is unique on
  `(data_asset_id, column_name)` for the same reason.
  `discovery_results`, by contrast, is append-only — one new row per job
  per asset touched, no uniqueness constraint beyond its own `id`.
- **Cascade behavior**: `discovery_jobs.data_source_id`,
  `data_assets.data_source_id`, `data_elements.data_asset_id`,
  `discovery_results.discovery_job_id`, and
  `discovery_results.data_asset_id` are all `ON DELETE CASCADE` — none of
  these has an independent lifecycle apart from its parent. This means a
  `discovery_results` row can be cascade-deleted via either of two parents
  (its job or its asset); Postgres supports this without restriction.
  `discovery_jobs.triggered_by_user_id` is `ON DELETE RESTRICT` (a job
  keeps a durable, non-deletable audit link to who ran it) and
  `data_elements.corrected_by_user_id` is `ON DELETE SET NULL` (a human
  correction's substance — the category itself — outlives the correcting
  user's account, only the "who" link is cleared).
- **`data_elements.classification_confidence` is `numeric(5,2)`** — a
  0–100 percentage with two decimal places, matching the brief's "98%"
  example precision.
- **Two aggregate roots, three children**: `data_sources` and
  `data_assets` are independently soft-deletable
  (`is_deleted`/`deleted_at`/`deleted_by`); `discovery_jobs`,
  `discovery_results`, and `data_elements` are children with no
  independent soft-delete, following the Module 5/6/7 precedent exactly.
- **Indexes**: every FK column gets one; `data_sources` additionally
  indexes `(organisation_id, source_type)`, `data_assets` is uniquely
  indexed as described above, and `data_elements` indexes
  `(organisation_id, classification_category)` to back the Classification
  dashboard's per-category counts and filters.
- **No new permission keys were pre-seeded for this module** (unlike
  Modules 5–7, whose permission keys were already present in Module 2's
  forward-looking catalogue) — Data Discovery is a Phase 4 module outside
  that original catalogue's scope. Six new keys were added in this
  module's own migration: `datasources.read`, `datasources.manage`,
  `discoveryjobs.read`, `discoveryjobs.manage`, `dataassets.read`,
  `classification.review` — see §4 for the seeding mechanism these follow.

## 12. Module 9 (Data Inventory & Processing Activities) — As Implemented

New tables: `data_categories`, `it_systems`, `data_collection_sources`,
`processors`, `recipients`, `retention_policies`, `data_inventory_items`,
`processing_activities`, `data_flows`, plus five implicit many-to-many
join tables: `processing_activity_data_categories`,
`processing_activity_it_systems`,
`processing_activity_data_collection_sources`,
`processing_activity_recipients`, `processing_activity_processors`. See
`docs/ARCHITECTURE.md` §19 and `docs/DATA_INVENTORY.md` for design
rationale; this section covers schema facts.

- **Every table here is an independent aggregate root** — none cascades
  from another via a required parent-child relationship the way, say,
  `evidence_versions` cascades from `evidence_items`. All nine are
  independently soft-deletable (`is_deleted`/`deleted_at`/`deleted_by`).
- **`data_inventory_items.data_category_id` is `ON DELETE RESTRICT`**
  (the one non-nullable, required catalogue reference on that table) —
  every other catalogue FK on `data_inventory_items` and
  `processing_activities` is nullable and `ON DELETE SET NULL`. In
  practice a hard delete never reaches these FKs at all — every deletion
  in this codebase is a soft delete — but the constraint is still correct
  defense in depth, and the application layer additionally blocks
  deleting a catalogue entry that is still referenced (409 Conflict, see
  each `Delete*CommandHandler`) rather than relying on the DB constraint
  alone.
- **The five processing-activity join tables have no dedicated entity
  class** — EF Core's implicit many-to-many (`HasMany().WithMany()`,
  explicitly named via `.UsingEntity(j => j.ToTable(...))` rather than
  left to default naming) generates a plain two-column join table for
  each, since no extra column is ever needed on the relationship itself.
  Composite primary key is `(catalogue_id, processing_activity_id)` on
  each; both FKs cascade — a join row has no meaning once either side is
  gone, which is unrelated to (and doesn't trigger) the soft-delete
  question above since these are pure link rows, not aggregate roots.
- **`processing_activities.data_subject_categories_json` and
  `.security_controls_json` are `jsonb`** — small per-record tag lists,
  the same pattern as `data_assets.indexes_json` (Module 8) and
  `remediation_tasks.evidence_json` (Module 6).
- **`data_inventory_items.sequence_number` /
  `processing_activities.sequence_number` / `data_flows.sequence_number`
  are Postgres identity columns**, the same atomic-under-concurrency
  pattern as every other module's human-facing display number
  (`DI-00001`/`PA-00001`/`DF-00001`, computed at the Application layer).
- **`data_flows` has two FKs into `it_systems`** (`from_it_system_id`,
  `to_it_system_id`) — configured explicitly in
  `DataFlowConfiguration` since EF Core's convention-based relationship
  discovery cannot infer which of two FKs to the same target table means
  what; neither has an inverse collection navigation on `ItSystem`.
- **No new permission keys were pre-seeded for this module** (the same
  situation as Module 8) — eight new keys were added in this module's own
  migration: `datainventory.read`, `datainventory.manage`,
  `processingactivities.read`, `.manage`, `.review`, `.approve`,
  `dataflows.read`, `dataflows.manage`.

## 13. Module 10 (Consent & Privacy Operations) — As Implemented

New tables: `data_principals`, `consent_purposes`, `privacy_notices`,
`consent_records`, `sla_policies`, `data_principal_requests`, plus one
implicit many-to-many join table: `privacy_notice_data_categories`. See
`docs/ARCHITECTURE.md` §20 and `docs/CONSENT_PRIVACY_OPERATIONS.md` for
design rationale; this section covers schema facts.

- **`consent_records` has no `is_deleted`/`deleted_at`/`deleted_by`
  columns** — the one aggregate root in this module that is not
  soft-deletable, modeled as compliance evidence (see
  `docs/CONSENT_PRIVACY_OPERATIONS.md` §3). Every other table here
  follows the standard soft-delete convention from §1.
- **`privacy_notices` has a unique index on `(organisation_id, code,
  version)`** — the versioning identity described in
  `docs/CONSENT_PRIVACY_OPERATIONS.md` §2; enforced at both the
  application layer (`ConflictException` in `CreatePrivacyNoticeCommand`)
  and the database.
- **`privacy_notice_data_categories` has no dedicated entity class** —
  the same implicit EF Core many-to-many shape as Module 9's five
  processing-activity join tables (`.UsingEntity(j =>
  j.ToTable("privacy_notice_data_categories"))`). Composite primary key
  is `(privacy_notice_id, data_category_id)`; both FKs cascade.
- **`sla_policies.request_type` is nullable** — `null` means "the
  catch-all policy for any request type with no more specific policy
  configured," not "unset." A partial unique constraint is not used;
  the one-active-policy-per-`request_type` rule (including the
  catch-all) is enforced at the application layer only (see
  `docs/CONSENT_PRIVACY_OPERATIONS.md` §5), the same choice Module 9
  made for catalogue-in-use checks.
- **`data_principal_requests.due_at` is nullable with no default** — it
  is `null` whenever no matching `SlaPolicy` exists at creation time;
  nothing in the schema or application code ever computes a fallback
  literal. See `docs/CONSENT_PRIVACY_OPERATIONS.md` §5.
- **`data_principal_requests.sequence_number` /
  `privacy_notices.sequence_number` / `consent_records.sequence_number`
  are Postgres identity columns**, the same atomic-under-concurrency
  pattern as every other module's human-facing display number
  (`DPR-00001`/`PN-00001`/`CR-00001`, computed at the Application
  layer).
- **`data_principal_requests.assigned_to_user_id` is `ON DELETE SET
  NULL`** into `users` — an unassigned request is a valid, expected
  state (it simply hasn't been picked up yet), unlike a required
  parent-child FK.
- **Eleven new permission keys were added in this module's own
  migration**: `privacynotices.read`, `.manage`, `.approve`,
  `consentpurposes.read`, `.manage`, `consent.read`, `.manage`,
  `dataprincipals.read`, `.manage`, `datarequests.read`, `.manage`.

## 14. Future Modules (not created yet)

Later phases add their own tables (`assets`,
`deletion_requests`, `vendors`,
`dpias`, `incidents`, `notification_logs`, `integration_connections`,
`ai_recommendations`, `monitoring_events`, ...) — each documented in its own
`docs/modules/<module>.md` and its own migration when that module is built,
following the conventions in §1 unchanged. `retention_policies` and
`processors` already exist as of Module 9 (see §12) — a later Vendor/
Processor Management module (roadmap items 28/29) extends rather than
replaces them.
