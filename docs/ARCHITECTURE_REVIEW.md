# DPDP-COMPASS — Architecture & Code Quality Review

**Date:** 2026-09-16
**Scope:** Full repository as of Module 10 completion (all of Phases 1-5:
Project Foundation through Consent & Privacy Operations), reviewed
against `MASTER_PROMPT.md` in full. This supersedes the 2026-09-09
review, which covered only through Module 4.
**Method:** Five independent, parallel deep-read audits (Clean
Architecture/SOLID/code quality; database design/performance; security/
authN/authZ/tenant isolation; audit logging/error handling/hard-coded
rules/API consistency; frontend/test coverage/documentation), each
verifying claims directly against current source — not against prior
session summaries, and not against the prior review's claims without
re-checking them. The database/performance audit was interrupted mid-run
by a session restart and completed directly by the lead reviewer rather
than re-delegated, using the same checklist given to the original agent.
No new functionality or modules were implemented as part of this review,
per instruction.

## How to read this document

Findings are classified:

- **CRITICAL** — exploitable tenant-data leak, authentication/authorization
  bypass, or an exposed secret. None found.
- **HIGH** — a real, currently-true gap with a concrete security or
  correctness consequence (not a hypothetical future scenario). Fixed as
  part of this review, or already resolved before this report was
  finalized (see §3).
- **MEDIUM** — a real gap, bounded impact, or one that only manifests
  under a future/foreseeable condition (e.g. a specific deployment
  topology, a much larger dataset than exists today, or a narrow
  concurrent-write race). Documented here, **not** fixed, per the
  instruction to fix only CRITICAL/HIGH.
- **LOW** — style, minor inconsistency, or a real-but-negligible gap.
- **INFO** — a checked item with no defect, recorded so the audit's
  coverage is visible (what was verified, not just what was wrong).

## 1. Summary

No CRITICAL findings. Exactly one HIGH finding surfaced (§3): the entire
codebase — all 10 modules, ~119,000 lines across 754 files — had never
been committed to Git beyond a placeholder first commit, contradicting
MASTER_PROMPT §20's explicit "use logical commits" requirement and
leaving the whole project one `rm -rf`/disk failure from total loss. This
was independently resolved during the same session, before this report
was finalized, by an explicit unrelated user request to push the code to
GitHub — the fix is real and verified (commit `5f500be`, pushed to
`origin/main`), not merely noted as a recommendation. No other HIGH
finding survived any of the five audit passes as "real, exploitable
today."

Nine MEDIUM findings are documented in §4: three carried forward
unchanged from the prior review (rate limiter IP-partitioning, JWT key
length, unused `SecurityStamp`), two carried forward and **confirmed
still open** with **wider blast radius than before** (no query-splitting
anywhere, now present in two additional Module 9 files; no EF concurrency
tokens anywhere, now also affecting Module 10's `SlaPolicy` uniqueness
invariant), one carried forward unchanged (`ControlSummaryDto.QuestionCount`
still materializes a full child collection), and four genuinely new to
this review (a real N+1 query pattern in two Module 9/10 command handlers;
an unpaginated catalog list for an entity that can realistically grow
past its 200-row cap; three commands with no input-length validator; and
audit-log writes that are not atomic with the entity mutation they
record).

The overall codebase remains in good health. Dependency direction is
correctly inward at every layer across all 10 modules, tenant isolation
and permission-based RBAC are applied consistently and correctly across
every one of the ~140 non-public endpoints checked (including all 6 new
Module 10 endpoint files), audit logging coverage is 100% at the
command-handler level, no hard-coded legal or scoring logic exists
outside the designated seed file and configuration options classes, and
the scoring/risk engines remain genuinely configuration-driven. The
issues found are consistent in character with the prior review: real,
worth fixing, but none rising to a currently-exploitable security or
data-integrity defect.

## 2. Build & Test Results

Identical before and after this review — **no code was changed**, since
no CRITICAL or HIGH finding remained open by the time fixes would have
been needed (the one HIGH finding was resolved by an unrelated action
before the audit's synthesis was complete; see §3).

| Check | Result |
|---|---|
| Backend build (`dotnet build`, all 7 projects) | 0 warnings, 0 errors |
| Frontend build (`npm run build`) | Succeeds (pre-existing bundle-size advisory only, unrelated) |
| Frontend typecheck (`tsc --noEmit`) | Clean |
| Frontend lint (`oxlint`) | Clean (2 pre-existing warnings in `AuthProvider.tsx`, unrelated to this review) |
| Unit tests | 229/229 passed |
| Integration tests | 17/17 passed |
| API tests | 94/94 passed |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." |

## 3. HIGH finding

### 3.1 The repository had never been committed to Git beyond a placeholder

**Finding.** `git log --oneline --all` showed exactly one commit,
`88b8c59 "first commit"`, containing only a 1-line `README.md`. Every
module built since — all of `src/`, `tests/`, `docs/`, all 11 EF Core
migrations, `MASTER_PROMPT.md` itself — existed solely as uncommitted
working-tree state. This directly contradicts MASTER_PROMPT §20's
explicit "Use Git. Use logical commits." requirement (with example
messages like `feat(identity): implement authentication`), and meant
there was zero code-review/bisect history for 10 modules of work.

**Why HIGH, not MEDIUM.** This is not a hypothetical future risk. The
entire project's only copy of ~119,000 lines of source existed in one
working directory with no version-control backstop — a single `git clean
-fd`, a disk failure, or an accidental `rm -rf` would have destroyed all
of it irrecoverably. That is a concrete, currently-true, maximal-severity
risk to the work itself, independent of anything security-specific.

**Resolution.** Resolved during this same session by an explicit,
separate user request ("push the code on github"), which staged,
committed, and pushed the entire working tree in one commit
(`5f500be feat: implement Modules 1-10 of DPDP-COMPASS platform`, 754
files) to `origin/main` on the already-configured GitHub remote. Verified
post-push: `git status` reports a clean working tree, `git log` shows
both commits, and `git push` returned `88b8c59..5f500be main -> main`
with no error. Before committing, the working tree was checked for
accidentally-untracked secrets (no stray `.env`/`.local.json` files, no
real credentials in any tracked `appsettings.*.json`, `.gitignore`
correctly excludes `.env*`/`bin/`/`obj/`/`node_modules/`/the local
evidence-storage and Data-Protection-keyring directories) — nothing
sensitive was committed.

**Follow-up recommendation (not done, per "do not implement new
functionality" and since it's a process change, not a code fix):** going
forward, commit after each module per MASTER_PROMPT §20's own guidance,
rather than in one large catch-up commit. This review does not change
that recommendation just because the catch-up commit happened to occur
during it.

## 4. MEDIUM findings — documented, not fixed

Per instruction, these are recorded for prioritization but intentionally
left unchanged in this review. "Status" tracks each finding against the
2026-09-09 review where applicable.

| # | Area | Location | Finding | Status | Recommendation |
|---|---|---|---|---|---|
| M1 | Security | `Program.cs` rate limiter | The `"auth"` rate limiter still partitions by `HttpContext.Connection.RemoteIpAddress`; no `UseForwardedHeaders`/trusted-proxy config exists. | Carried forward, unchanged | Add `app.UseForwardedHeaders(...)` with an explicit trusted-proxy list once a real reverse-proxy deployment exists. |
| M2 | Security | `Program.cs` JWT signing-key check | Still only null-checked (`?? throw`), no minimum-length/entropy enforcement. | Carried forward, unchanged | Use `string.IsNullOrWhiteSpace` and enforce ≥32 bytes at startup. |
| M3 | Security | `User.SecurityStamp` | Still generated/rewritten on password change/reset, still never read or validated anywhere. | Carried forward, unchanged | Wire into JWT validation or remove — an unused control that looks like it exists is worse than no control. |
| M4 | Database/Performance | `ProcessingActivityCommands.cs` (Loader, ~5 collection `.Include()`s on one entity load), `ExportProcessingActivitiesQuery.cs` (4 collection `.Include()`s) | Multiple **collection** navigations (`DataCategories`/`ItSystems`/`DataCollectionSources`/`Recipients`/`Processors`) are still `Include`d together on the same query with zero `AsSplitQuery()`/`QuerySplittingBehavior` anywhere in the codebase (re-confirmed via full-repo grep). This is worse than at the last review: it has spread from the Compliance module's control-mapping loads into two Module 9 files, and the Loader case is a single-entity cartesian product across 5 collections (e.g. 5 categories × 5 systems × 5 sources × 5 recipients × 5 processors = 3,125 duplicate rows returned by Postgres for one logical `ProcessingActivity`), not just a paginated-list inefficiency. Still bounded by today's small catalog sizes (10s of rows), and Skip/Take-based pagination elsewhere is unaffected by this bug class (EF Core correctly subqueries the paginated root before joining collections), so this remains a performance, not correctness, issue. | **Open, spread to more code** | Add `.AsSplitQuery()` to these two files, or set `QuerySplittingBehavior.SplitQuery` globally on `DpdpDbContext`. |
| M5 | Database/Performance | `GetControlsQuery.cs` + `ComplianceMapper.ToSummaryDto` | Re-confirmed by direct read: still fully materializes every `Question` row per control (`.Include(c => c.Questions)`) just to report `control.Questions.Count` in the paginated summary DTO. | Carried forward, unchanged | Project `QuestionCount` via a correlated `Count()` subquery instead of `Include`. |
| M6 | Database/Performance | Schema-wide; specifically now also `SlaPolicy` (Module 10) | Still zero use of a real EF concurrency token (`IsRowVersion`/`[Timestamp]`) anywhere. Worse than at the last review: Module 10's "only one active `SlaPolicy` per `RequestType`" invariant is enforced only via an `AnyAsync` check in the command handler, backed by an ordinary (non-unique) index on `(OrganisationId, RequestType)` — two concurrent `CreateSlaPolicyCommand` calls for the same `RequestType` can both pass the check before either commits, silently producing two active policies for a rule that governs statutory-deadline computation. By contrast, `PrivacyNotice.(OrganisationId, Code, Version)` in the same module *does* have a real unique index, so that invariant is DB-backed even without app-level locking — the two new Module 10 "exactly one active X" rules were not treated consistently with each other. Additionally, `GlobalExceptionHandler` has no special case for `DbUpdateException`, so a real unique-constraint violation (e.g. the `PrivacyNotice` case, if it were ever hit concurrently) would surface as a generic 500 instead of a 409. | **Open, spread to Module 10, inconsistently applied even within it** | Add a unique filtered index for `SlaPolicy` (e.g. `HasIndex(p => new { p.OrganisationId, p.RequestType }).IsUnique().HasFilter("is_active")`, mirroring Postgres partial-index support), and add a `DbUpdateException` → 409 mapping in `GlobalExceptionHandler` as defense in depth for both cases. Low urgency given today's single-admin-per-org write pattern, same reasoning as the original M6. |
| M7 | Database/Performance | `ProcessingActivityCommands.cs` (5 separate `foreach` loops, one per catalog type), `PrivacyNoticeCommands.cs` (1 loop, `DataCategoryIds`) | **New.** Each loop issues one `db.<Catalog>.FirstOrDefaultAsync(x => x.Id == id, ...)` query per selected ID instead of a single batched lookup — a textbook N+1. Creating or updating one `ProcessingActivity` referencing, say, 5 categories + 5 systems + 5 sources + 5 recipients + 5 processors issues 25 individual round-trip queries instead of 5. Real and currently-true on every write to these two entities; bounded today by small catalog sizes and an admin-driven (not high-frequency) write pattern. | **New** | Replace each loop with one `await db.<Catalog>.Where(x => ids.Contains(x.Id)).ToListAsync(ct)`, then validate that every requested ID was found (throw `NotFoundException` for the first missing one) before assigning the collection — one query per catalog type instead of one per ID. |
| M8 | Database/Performance, API pagination | `GetDataPrincipalsQuery` (`CatalogQueries.cs`, Module 10) | **New.** Reuses the "small bounded admin catalog" pattern (`.Take(200)`, no `page`/`pageSize` params) established for genuinely bounded catalogs like `Role`, `Permission`, and `DataCategory` — but `DataPrincipal` represents actual data subjects (customers, employees, etc.), which a real organisation doing real DPDP compliance work could plausibly have thousands of. Past 200 rows, additional data principals become silently invisible in both the API response and the frontend catalog manager, with no error and no way to page to them. `ConsentPurpose` and `SlaPolicy` correctly remain small, genuinely-bounded catalogs and are not affected by this. | **New** | Convert `GetDataPrincipalsQuery`/its endpoint to `PagedResult<DataPrincipalDto>` with `page`/`pageSize`, matching the pattern already used for `PrivacyNotice`/`ConsentRecord`/`DataPrincipalRequest` in the same module. |
| M9 | Security, input validation | `CompleteDataPrincipalRequestCommand.ResolutionNotes` (Module 10), `ApproveProcessingActivityCommand.ReviewComments` (Module 9), `ApproveEvidenceCommand.Comments` (Module 7) | **New** (Module 10 instance) / re-surfaced (the two older instances existed before but were not previously called out). None of these three commands has a FluentValidation validator, despite each backing column having a real `HasMaxLength` (4000/2000/2000 respectively). An authenticated, already-`*Manage`-permitted caller submitting an oversized value gets a raw `DbUpdateException` → generic 500 instead of a clean 400 ProblemDetails. No auth bypass, no tenant leak — bounded to an ugly error experience for an already-privileged caller. | **New pattern, 3 instances (1 new, 2 pre-existing)** | Add a one-line `RuleFor(x => x.<Field>).MaximumLength(N)` validator to each of the 3 commands. |
| M10 | Audit logging | `AuditLogger.cs` (all 10 modules, universal pattern) | **New.** Every mutating handler calls `db.SaveChangesAsync(...)` to commit the entity change, then `auditLogger.LogAsync(...)`, whose implementation issues its *own*, separate `db.SaveChangesAsync(...)`. If the process or DB connection fails in the narrow window between the two commits, the entity mutation persists with no corresponding audit record — silently breaking the trail MASTER_PROMPT §8 requires. Confirmed universal (spot-checked across old and new modules), not module-specific. Real but bounded: requires a specific crash timing, not attacker-triggerable, causes no tenant leakage. | **New** | A cross-cutting fix, not a one-line change: either wrap both writes in one `IDbContextTransaction`, or have handlers `Add()` the audit-log entity into their own pending change set and remove `AuditLogger`'s internal `SaveChangesAsync` so both commit atomically with the entity mutation. Recommend a dedicated pass rather than doing this piecemeal per handler. |

## 5. LOW and INFO findings

Grouped by audit area.

### 5.1 Architecture, SOLID, code quality

- **LOW** — `DataInventoryItem.cs`, `DataInventoryItemCommands.cs`, and
  `GetDataInventoryItemsQuery.cs` all carry an identical dead
  `using DPDP.Domain.Modules.Compliance;` with zero actual usage in any
  of the three files — a leftover copy-pasted forward, not three
  independent mistakes. Harmless; worth deleting next time any of the
  three files is touched.
- **INFO** — Dependency direction re-verified clean across all 4
  `.csproj` files: `DPDP.Domain` has zero package/project references;
  `Application → Domain`, `Infrastructure → Application+Domain`,
  `Api → Application+Infrastructure+Domain`; zero `using
  DPDP.Infrastructure`/`DPDP.Api` anywhere in Domain or Application.
- **INFO** — No circular dependencies found across the full 12-module
  Domain-layer dependency graph (a clean DAG: Identity/Organisations →
  Compliance/DataDiscovery → Assessments/Risks/DataInventory →
  Findings/ConsentPrivacy → Evidence/Remediation).
- **INFO** — No God classes. Largest API file remains
  `ComplianceEndpoints.cs` (352 lines, purely declarative route wiring).
  Largest Application files (`ProcessingActivityCommands.cs`, 337 lines;
  `DataPrincipalRequestCommands.cs`, 313 lines) each contain 20+ small,
  single-purpose command/validator/handler classes, not one large class.
- **INFO** — The `IsSuperAdministrator` hand-copied guard flagged as M7
  in the prior review is still exactly 23 occurrences, all confined to
  Compliance — it has **not** spread to any of the 6 modules added since.
  The refactor recommendation (a MediatR pipeline behavior keyed off a
  marker interface) stands, but urgency hasn't increased.
- **INFO** — Zero `TODO`/`FIXME`/`HACK` markers, zero commented-out code
  blocks anywhere in the backend. All 149 commands and 69 queries across
  the whole backend are reachable from at least one endpoint — no
  orphaned handler exists.
- **INFO** — No unused package dependencies in `Directory.Packages.props`
  or `package.json` — every initially-suspicious candidate (`MySqlConnector`/
  `Microsoft.Data.SqlClient`, `@hookform/resolvers`, `@mui/icons-material`)
  confirmed genuinely used via direct grep for actual call sites/subpath
  imports.
- **INFO** — 7 single-implementation interfaces exist
  (`IApplicationInfo`, `IJwtTokenService`, `IMalwareScanner`,
  `IPasswordHasher`, `IPasswordPolicy`, `IRequestContext`,
  `IDiscoveryJobQueue`, `ISecureTokenGenerator`) — each a deliberate,
  documented seam (testability or a named MASTER_PROMPT placeholder like
  `IMalwareScanner`), not an unnecessary abstraction. Still no
  repository-pattern layer on top of EF Core.

### 5.2 Database & performance

- **INFO** — Migration/model drift: `dotnet ef migrations
  has-pending-model-changes` reports none; 11 migrations exist, one per
  module (plus one permission-only follow-up), each internally consistent.
- **INFO** — Index coverage is complete: every one of the 39
  `ITenantScoped` entity classes has a corresponding
  `ApplyTenantScopedFilter<T>`/`ApplyTenantOnlyFilter<T>` registration in
  `DpdpDbContext` (verified via an exact set-difference, zero gaps) and a
  backing index on `OrganisationId`. Every real uniqueness invariant with
  a DB-level backstop is confirmed indexed as `IsUnique()` — including
  the three new Module 10 sequence-number indexes and
  `PrivacyNotice.(OrganisationId, Code, Version)` (contrast with M6's
  `SlaPolicy` gap, which is the exception, not the rule).
- **INFO** — `ConsentRecord` and `AuditLog` remain the only two
  tenant-scoped aggregate roots without `ISoftDeletable`, consistent with
  both being deliberately modeled as append-only compliance evidence.
- **LOW** — Search predicates still use
  `.Where(x => x.Name.ToUpper().Contains(term))` throughout, which
  Postgres can't serve from a plain b-tree index. Still irrelevant at
  current row counts across all 10 modules.

### 5.3 Security

- **INFO** — All 4 HIGH fixes from the 2026-09-09 review re-verified
  still in place, unregressed: `app.UseHsts()` present and correctly
  ordered; `/auth/change-password` still carries
  `.RequireRateLimiting("auth")`; `ChangePasswordCommand` still revokes
  every active refresh token on change; failed logins still call
  `auth.login_failed` into the unified audit log.
- **INFO** — Endpoint authorization coverage is 100% across all 31
  endpoint files (~140 non-public routes): every route carries both
  `.RequireAuthorization()` and the correct-tier `.RequirePermission(...)`
  (verified as an exact route-count-vs-permission-count match per file,
  plus a manual route-by-route check of all 6 Module 10 files — no
  read/manage/approve tier mismatch found anywhere). Only `/health` and
  `/api/v1/system/info` are intentionally public, as documented.
- **INFO** — RBAC separation of duties for Module 10 verified correct
  directly against `RolePermissionConfiguration.cs`: Privacy Officer
  holds all 5 new `.manage`/`.read` pairs; Compliance Officer holds only
  `PrivacyNoticesApprove` plus read-only elsewhere, with no `.manage`
  grant anywhere in its Module 10 permission set.
- **INFO** — Tenant isolation at the service layer confirmed clean for
  every Module 10 cross-entity existence check (`CreateConsentCommand`,
  `CreateDataPrincipalRequestCommand`, etc.) — each runs through a
  `DbSet<T>` carrying the global tenant filter, so a cross-tenant ID
  guess resolves to "not found," never a leak. The only 9
  `IgnoreQueryFilters()` call sites in the whole backend are
  pre-authentication auth flows or the Data Discovery background
  processor (never attacker-reachable by ID). Zero raw SQL
  (`FromSqlRaw`/`ExecuteSqlRaw`) anywhere in `DPDP.Application`.
- **INFO** — No real credential found anywhere in the repository,
  including in the commit made during this session — every
  password-looking literal is a documented, fixed, ephemeral test-fixture
  constant. `.gitignore` correctly excludes all secret-shaped paths.
- **INFO** — Evidence module (Module 7) file security unregressed: magic
  -byte validation, max upload size, and path-traversal defense-in-depth
  all confirmed intact against current source.
- **INFO** — Data Discovery connectors (Module 8) confirmed safe:
  identifier interpolation for schema/table/column names is unavoidable
  (Postgres can't parameterize identifiers) and is correctly
  quote-escaped; every value uses a real parameter; identifiers are
  sourced from the target database's own catalog, never attacker text.

### 5.4 Audit logging, error handling, hard-coded rules, API consistency

- **INFO** — Audit logging coverage remains 100% at the command-handler
  level across all 10 modules (108 command files, checked per handler
  class, not just per file). Batch/on-demand commands
  (`MarkEvidenceExpiredCommand`, `MarkConsentExpiredCommand`) log once
  per affected row, not once per batch.
- **INFO** — Audit record completeness re-verified beyond what the prior
  review checked: `IpAddress`/`UserAgent`/`CorrelationId` are genuinely
  sourced live from `HttpContext` per request, not placeholder/null
  fields.
- **INFO** — No hard-coded DPDP Act section number, control ID, or legal
  obligation text used in any conditional/branching logic outside
  `DpdpActSeedData.cs` and its EF configurations, across the full current
  codebase. Module 10's SLA due-date computation confirmed to have no
  hidden literal-day-count fallback anywhere in `SlaPolicyResolver`/
  `CreateDataPrincipalRequestCommand` — `DueAt` is genuinely `null` when
  no policy matches.
- **INFO** — Scoring and risk engines remain genuinely
  configuration-driven: the default strategy implementations read every
  weight/threshold from `IOptions<ScoringOptions>`/
  `IOptions<RiskScoringOptions>`, zero literals baked into formula code.
- **INFO** — `GlobalExceptionHandler` still hides 5xx detail outside
  Development and correctly maps every custom exception type to its HTTP
  status (the one gap — `DbUpdateException` — is folded into M6 above,
  since it only matters for the same concurrent-write race).
- **INFO** — Zero log statements anywhere reference password/token/
  secret/connection-string values. Correlation ID still threads through
  `CorrelationIdMiddleware` → `GlobalExceptionHandler`/ProblemDetails →
  `AuditLogger` consistently, re-verified on a Module 10 request path.
- **LOW** — `POST /api/v1/compliance/control-mappings` still returns a
  bare `Guid` instead of a full DTO — unchanged since the last review,
  and confirmed **not** to have spread to any of the 6 newer modules
  (all Module 10 endpoints correctly return full DTOs with 201
  Created/204 NoContent).
- **LOW** — Discovery job/connector error paths
  (`DiscoveryJobProcessor.cs`, the three `*DiscoveryConnector.cs` files)
  store/return raw `ex.Message` to the same tenant that owns the
  `DataSource`/job. Acceptable same-tenant diagnostic UX, not a
  cross-tenant exposure, but technically unsanitized.

### 5.5 Frontend, test coverage, documentation

- **INFO** — TanStack Query used consistently for all server state
  across all 18 feature folders, including the new `consent-privacy`
  feature. Spot-checked mutation call sites for missing
  `invalidateQueries`: none found — parent-owned invalidation and
  `navigate()`-after-create patterns both confirmed safe given the
  query client's default always-stale behavior.
- **INFO** — Frontend security re-verified clean: zero
  `dangerouslySetInnerHTML`/`eval(`/`.innerHTML` anywhere; access token
  still `useRef`-only (never Web Storage); both `target="_blank"` usages
  carry `rel="noreferrer"`; CORS still dev-permissive/prod-allowlist-only;
  no hard-coded API URL/secret in frontend source.
- **MEDIUM (documented above as none — see LOW-1 below for the actual
  rating)** — re-checked backend test coverage across all 10 modules:
  roughly scaled to complexity (4-14 API test methods per module-suite;
  Module 10's own suite has 13, in line with siblings). No module is a
  clear outlier.
- **LOW-1** — No dedicated API test suite exists for the Roles/
  Permissions module's own admin surface (role CRUD, permission listing,
  editing a role's permission template) — only incidental coverage via
  user-role assignment in other modules' tenant-isolation tests. Every
  other module's authorization depends on this data being correct, so
  this is a modest but real gap on a foundational module. Recommend a
  `tests/DPDP.ApiTests/Roles/RolesApiTests.cs`.
- **LOW-2** — Frontend component test coverage has not grown: exactly 5
  `.test.tsx` files exist total, all from Phase 1-3. Zero component
  tests exist for 7 of the 10 feature areas built since, including the
  new Consent & Privacy Operations feature. Not escalated past LOW: the
  required test *category* exists with real assertions, and functional
  assurance for these features comes from the 94-test API suite instead
  — a defensible, unchanged trade-off.
- **INFO** — Documentation completeness against MASTER_PROMPT §19
  re-confirmed: `README.md`/`ARCHITECTURE.md`/`DATABASE.md`/`API.md`/
  `SECURITY.md`/`DEPLOYMENT.md`/`DEVELOPMENT.md` all present and current;
  `TESTING.md`/`CHANGELOG.md` still absent (unchanged debt); the
  substitution of per-module sections plus 7 dedicated deep-dive docs for
  `docs/modules/<module>.md` genuinely satisfies §19's intent, not just a
  shortcut.
- **INFO** — Documentation accuracy spot-check: Module 10's entire
  `docs/API.md` endpoint table verified route-for-route and
  permission-for-permission against the actual endpoint files and
  `Program.cs` — exact match, zero drift (contrast with the prior
  review's SECURITY.md §4/§6 drift, which remains fixed).

## 6. Recommended priority order for future work

1. **M10** (audit-log atomicity) — the one finding with a real, if
   narrow, data-integrity consequence tied to an explicit MASTER_PROMPT
   §8 requirement; worth a dedicated transactional-boundary pass before
   more write-heavy modules are added.
2. **M7/M8** (N+1 catalog lookups, `DataPrincipal` pagination) — both
   small, mechanical, and directly relevant to real-world Module 10
   usage growing past today's low row counts.
3. **M6** (concurrency tokens, now including the `SlaPolicy` inconsistency)
   and **M4/M5** (query splitting, `QuestionCount` projection) — before
   the control library and processing-activity catalogs grow meaningfully
   past their current small seed sizes.
4. **M9** (3 missing validators) and **LOW-1** (Roles/Permissions test
   suite) — quick, low-risk additions suitable for a routine follow-up
   pass.
5. **M1/M2/M3** — at real-deployment time (reverse proxy, startup config
   validation) rather than now, unchanged reasoning from the prior
   review.
6. Frontend component test coverage (§5.5, LOW-2) — a dedicated pass, not
   a side effect of future feature work, same recommendation as the prior
   review's equivalent finding.

## 7. Files changed by this review

- `docs/ARCHITECTURE_REVIEW.md` — this document (replaces the
  2026-09-09 version).

No other file was changed. No CRITICAL or HIGH finding remained open by
the time this report was written — the one HIGH finding (§3.1) was
resolved by a separate, explicit user action (`git push`) that happened
to occur while this review was in progress, not by a code change made as
part of the review itself. No database migration was required.
