# DPDP-COMPASS — Architecture & Code Quality Review

**Date:** 2026-09-09
**Scope:** Full repository as of Module 4 completion (Project Foundation,
Identity/RBAC, Organisation Management, DPDP Compliance Framework &
Control Library), reviewed against `MASTER_PROMPT.md` in full.
**Method:** Five independent, parallel deep-read audits (Clean
Architecture/code quality; database design/performance; security/tenant
isolation/RBAC/auth; audit logging/validation/error handling/hard-coded
logic; API consistency/frontend/test coverage/documentation), each
verifying claims directly against current source — not against prior
session summaries. No new functionality or modules were implemented as
part of this review, per instruction.

## How to read this document

Findings are classified:

- **CRITICAL** — exploitable tenant-data leak, authentication/authorization
  bypass, or an exposed secret. None found.
- **HIGH** — a real, currently-true gap with a concrete security or
  correctness consequence (not a hypothetical future scenario). Fixed as
  part of this review (see §3).
- **MEDIUM** — a real gap, bounded impact, or one that only manifests
  under a future/foreseeable condition (e.g. a specific deployment
  topology or a much larger dataset than exists today). Documented here,
  **not** fixed, per the instruction to fix only CRITICAL/HIGH.
- **LOW** — style, minor inconsistency, or a real-but-negligible gap.
- **INFO** — a checked item with no defect, recorded so the audit's
  coverage is visible (what was verified, not just what was wrong).

## 1. Summary

No CRITICAL findings. No pre-existing HIGH findings survive from any of
the five audit passes as "real, exploitable today" — but four findings
that the individual audits calibrated as MEDIUM were, on consolidation,
reclassified to HIGH and fixed (see §3), because each one is a concrete,
currently-true gap tied directly to an explicit MASTER_PROMPT requirement,
with a small, low-risk, precedented fix already available in the
codebase's own patterns. Everything else — 6 further MEDIUM findings, all
LOW and INFO findings — is documented in §4/§5 and left unfixed, per the
"fix only CRITICAL and HIGH" instruction.

The overall codebase is in good health. Dependency direction is correctly
inward at every layer, module boundaries hold, tenant isolation and
permission-based RBAC are applied consistently and correctly across every
handler and endpoint checked, no hard-coded legal or scoring logic exists
outside the designated seed file, and the database/migration layer is
clean and consistent. The issues found are the kind a careful, disciplined
build still accumulates — a few doc/reality drifts, a couple of
inconsistencies between two similar code paths, and standard
production-hardening items not yet needed for a dev-only deployment.

## 2. Build & Test Results

**Before fixes** (baseline, confirmed clean prior to any change in this
review):

| Check | Result |
|---|---|
| Backend build (`dotnet build`, all 7 projects) | 0 warnings, 0 errors |
| Frontend build (`npm run build`) | Succeeds (pre-existing bundle-size advisory only, unrelated) |
| Unit tests | 15/15 passed |
| Integration tests | 10/10 passed |
| API tests | 29/29 passed |
| Frontend tests | 7/7 passed |

**After the four HIGH fixes in §3** (final verification):

| Check | Result |
|---|---|
| Backend build (`dotnet build`, all 7 projects) | 0 warnings, 0 errors |
| Unit tests | 15/15 passed |
| Integration tests | 10/10 passed |
| API tests | 29/29 passed |
| Frontend tests | 7/7 passed |
| Dev database state | Verified clean (13 seeded controls, no leaked test rows) |

No test needed to change to accommodate a fix — all four fixes are
additive (a new middleware call, a new revoke-on-change-password step, a
new rate-limit attribute, a new audit-log call, and two documentation
corrections) and none altered any existing behavior that a test asserted
on.

## 3. HIGH findings — fixed in this review

### 3.1 `docs/SECURITY.md` §4 contradicted the actual implementation

**Finding.** §4 ("Transport & Headers") described the *original,
pre-implementation design*: refresh token in an `HttpOnly`/`Secure`/
`SameSite=Strict` cookie, ASP.NET Core antiforgery for CSRF, HSTS enabled,
rate limiting "per-IP and per-account." None of this matches what Module 2
actually built (confirmed by reading `AuthProvider.tsx`, `Program.cs`, and
`SecurityHeadersMiddleware.cs` directly): the refresh token is in
`localStorage`, there is no cookie-based flow and therefore no CSRF
antiforgery, HSTS was never enabled, and the rate limiter partitions by
client IP only. §12 of the same document already described the real
design correctly and even said so explicitly ("not the original
cookie-based sketch in §4") — but §4 itself was never corrected, leaving a
document that says two contradictory things about the same control.

**Why HIGH, not MEDIUM.** This is not a hypothetical future risk — it is a
security document that is factually wrong *right now*. Anyone reading §4
in isolation (the section named for exactly this topic) would form an
incorrect model of how authentication transport security works in this
system.

**Fix.** Rewrote §4 to state the actual implementation, cross-reference
§12 for the full trade-off discussion, and explicitly flag the two real
gaps it surfaced (HSTS, rate-limiter-per-IP-only) rather than claiming
they're already handled. No code behavior changed by this part of the fix.

### 3.2 `docs/SECURITY.md` §6 claimed a database control that does not exist

**Finding.** §6 stated that `audit_logs` tamper-resistance is "enforced
via a dedicated Postgres role/grant" restricting the application's
database role to `INSERT`/`SELECT` only. No migration, SQL script, or
deployment file anywhere in the repository creates such a role or runs any
`REVOKE`. The single configured application database user owns the schema
(it ran the migrations) and therefore currently has full `UPDATE`/`DELETE`
rights on `audit_logs`.

**Why HIGH, not MEDIUM.** A security document asserting an access control
exists when it does not is worse than one that honestly documents the gap
— someone could rely on this claim for a compliance or audit sign-off.

**Fix.** Rewrote §6 to state the true current state (append-only by
application convention — verified no code path ever calls `.Update(...)`/
`.Remove(...)` on `db.AuditLogs` — but not yet enforced at the database
level) and named the concrete follow-up (a dedicated, minimally-privileged
Postgres role, provisioned outside EF Core migrations since the
migration-running role needs DDL rights). Implementing that role was
deliberately **not** done as part of this review — it is new
infrastructure, not a fix to existing code, and out of scope for a
documentation-and-fixes audit pass per the "do not implement new
functionality" instruction.

### 3.3 HSTS was never enabled despite being documented as on

**Finding.** `docs/SECURITY.md` claimed "HSTS enabled." `app.UseHsts()` is
never called anywhere in `Program.cs`, and `SecurityHeadersMiddleware`
never sets a `Strict-Transport-Security` header. MASTER_PROMPT §7 lists
HTTPS and secure headers as baseline requirements for this
"security-sensitive enterprise application."

**Why HIGH, not MEDIUM.** This is a direct, checkbox-level gap against an
explicit MASTER_PROMPT security requirement (not a subtle emergent risk),
already (incorrectly) documented as done, and the fix is a single,
well-understood, zero-risk ASP.NET Core middleware call.

**Fix.** Added `app.UseHsts()` in `DPDP.Api/Program.cs`, in the same
`!IsDevelopment()` block that already gates `UseHttpsRedirection()`, in
the conventional order (HSTS before the HTTPS redirect).

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

### 3.4 `ChangePasswordCommand` left every other session alive

**Finding.** `ResetPasswordCommand` (the forgot-password flow) explicitly
revokes every active refresh token for the user on a successful reset —
correctly treating "the user just proved they should regain sole control
of this account" as a reason to kill every other session.
`ChangePasswordCommand` (the logged-in, self-service flow) did not do
this: after changing a password, every other device/browser session
remained valid until its access token naturally expired (up to 15 minutes)
and its refresh token was next used.

**Why HIGH, not MEDIUM.** This is a real, current inconsistency between
two code paths that exist for the same purpose (proving account control to
set a new password), one of which correctly implements the session
management MASTER_PROMPT §7 requires and the other doesn't. A user who
changes their password specifically because they suspect their account is
compromised would not actually evict the attacker's session by doing so —
undermining the primary reason someone changes a password in that
scenario.

**Fix.** `ChangePasswordCommand.cs` now revokes every active refresh token
for the user in the same transaction as the password change, mirroring
`ResetPasswordCommand`'s existing, already-tested pattern exactly (added
`IDateTimeProvider` to the handler's constructor to get `now` the same
way).

### 3.5 `/auth/change-password` was the only password-verifying endpoint with no rate limit

**Finding.** `/auth/login`, `/auth/refresh`, `/auth/forgot-password`, and
`/auth/reset-password` all carry `.RequireRateLimiting("auth")`.
`/auth/change-password` — which also verifies a secret (the caller's
current password) before proceeding — did not, and
`ChangePasswordCommandHandler` tracks no failed-attempt counter of its own
(unlike login's `FailedLoginCount`/lockout).

**Why HIGH, not MEDIUM.** Anyone holding a valid access token (stolen via
XSS, a leaked token, a compromised or unattended device) could attempt to
guess the account's current password an unlimited number of times with no
throttling at all, in order to seize full, durable control of the account
(change the password to one they know, which — per §3.4 above — now also
evicts the legitimate user). This is a concrete brute-force path against a
password-verification endpoint, the exact class of gap rate limiting on
the *other* four password endpoints already exists to close.

**Fix.** Added `.RequireRateLimiting("auth")` to the `/auth/change-password`
route registration in `AuthEndpoints.cs`, reusing the same named policy
every other password-touching endpoint already uses.

### 3.6 Failed logins were invisible to the unified audit log

**Finding.** MASTER_PROMPT §8 explicitly lists "failed login" among the
actions the audit trail must track. Failed, locked-out, and
disabled-account login attempts were recorded only in the separate
`LoginHistory` table (via `RecordLoginHistoryAsync`) — never via
`IAuditLogger` — while a *successful* login is correctly logged to both
sinks. A future `GET /audit-logs` endpoint reading only `audit_logs` (the
sink §8's schema describes) would show every successful login and no
failed ones, an asymmetry an auditor or incident responder would not
expect.

**Why HIGH, not MEDIUM.** This is a named, explicit requirement in
MASTER_PROMPT §8, currently unmet for the specific event type §8 calls out
by name, with a trivial and precedented fix (the exact same
`auditLogger.LogAsync` call pattern already used by 40+ other handlers).

**Fix.** `RecordLoginHistoryAsync` in `LoginCommand.cs` now also calls
`auditLogger.LogAsync("auth.login_failed", ...)` whenever `succeeded` is
false, carrying the attempted email and failure reason (never the
password) as `newValue`. Consolidated into the one shared method (called
from all four failure sites: no such account, locked out, disabled,
bad password) rather than duplicated at each call site.

## 4. MEDIUM findings — documented, not fixed

Per instruction, these are recorded for prioritization but intentionally
left unchanged in this review.

| # | Area | Location | Finding | Recommendation |
|---|---|---|---|---|
| M1 | Security | `Program.cs` rate limiter | The `"auth"` rate limiter partitions by `HttpContext.Connection.RemoteIpAddress`. No `UseForwardedHeaders` / trusted-proxy configuration exists. Once deployed behind the reverse proxy MASTER_PROMPT's deployment target implies, every request will appear to share the proxy's IP, collapsing the limiter back to one shared bucket. | Add `app.UseForwardedHeaders(...)` with an explicit trusted-proxy list before `UseRateLimiter()` when a real reverse-proxy deployment is set up. Not actionable today — no reverse-proxy config exists yet in `deployment/`. |
| M2 | Security | `Program.cs:57-59` JWT signing-key check | `builder.Configuration["Jwt:SigningKey"] ?? throw ...` only catches `null`, not an empty or very short string; there is no minimum-length/entropy enforcement. | Use `string.IsNullOrWhiteSpace` and enforce a minimum key length (≥32 bytes for HMAC-SHA256) at startup. |
| M3 | Security | `User.SecurityStamp` | Generated at signup and rewritten on password change/reset, but never read or validated anywhere (not in JWT claims, not checked by any auth code) — implies a "stamp-based session invalidation" mechanism that doesn't actually exist. | Either wire it into JWT validation (embed in claims, check on each request) or remove it — leaving it unused implies a control that isn't there. |
| M4 | Database/Performance | `GetControlByIdQuery.cs`, `UpdateControlCommand.cs`, `GetRequirementsQuery.cs`, `UpdateRequirementCommand.cs` | Each loads 2+ sibling/nested collection navigations in one query with no `QuerySplittingBehavior` configured — EF Core's own "multiple collection include" warning was observed live during smoke testing. Negligible today (1 mapping/1 question per seeded control) but multiplies as the framework grows. | Add `.AsSplitQuery()` to these handlers, or configure `QuerySplittingBehavior.SplitQuery` globally on `DpdpDbContext`. |
| M5 | Database/Performance | `GetControlsQuery.cs` + `ComplianceMapper.ToSummaryDto` | Paginated control list fully materializes every question row per control (via `.Include(c => c.Questions)`) just to report an integer `QuestionCount` in the summary DTO — `GetControlCategoriesQuery` already does this correctly via a count subquery instead. | Project `QuestionCount` via a correlated `Count()` subquery instead of `Include`. |
| M6 | Database/Performance | Schema-wide | No entity uses a real EF concurrency token (`IsRowVersion`/`[Timestamp]`). `Control.Version` is an app-level counter with no concurrency check — two concurrent `PUT /controls/{id}` requests can silently produce a lost update. Same class of risk for `FrameworkVersion.IsCurrent`/`OrganisationLocation.IsPrimary`, whose "exactly one true" invariants are enforced only in handler logic. | Add a `byte[] RowVersion` concurrency token to `Control`, `FrameworkVersion`, and `OrganisationLocation`; turn a silent overwrite into a `DbUpdateConcurrencyException` → 409. Low urgency given today's single-Super-Admin write pattern. |
| M7 | Architecture | `DPDP.Application/Modules/Compliance/Commands/**/*.cs` (23 files) | The `if (!currentUser.IsSuperAdministrator) throw new ForbiddenException(...)` guard is hand-copied verbatim into all 23 Compliance command handlers. Correct everywhere today, but nothing in the type system stops a *future* handler from being added without it. | Introduce a MediatR pipeline behavior keyed off a marker interface (e.g. `IRequireSuperAdministrator`), mirroring the existing `ValidationBehavior<TRequest,TResponse>` — turns "must remember to copy this" into "cannot compile/run without it." A refactor, not a bug fix — deliberately not done in this review. |
| M8 | Documentation | `docs/` | `TESTING.md`, root `CHANGELOG.md`, and every `docs/modules/<module>.md` file required by MASTER_PROMPT §19 (and promised for Phase 1 in `docs/PROJECT_PLAN.md`) are entirely absent. | Real, self-acknowledged debt across 4 shipped modules. Worth a dedicated documentation pass before Phase 2 grows further — deliberately not created as a side effect of this audit (writing them well requires the same module-by-module care as the code). |
| M9 | Frontend/Deployment | Frontend delivery | No Content-Security-Policy exists for the SPA's own served HTML (only the JSON API sets a CSP, correctly `default-src 'none'` for itself). No reverse-proxy/static-hosting config exists yet in `deployment/` to attach one to the real page. | Add a frontend-appropriate CSP when a real static-hosting/reverse-proxy config is committed. Low risk today — no `dangerouslySetInnerHTML`/`eval`/unescaped-HTML rendering found anywhere in the frontend. |

## 5. LOW and INFO findings

Grouped by audit area; these required no action and mostly confirm the
codebase is doing the right thing.

### 5.1 Architecture & code quality (LOW/INFO)

- **INFO** — `DPDP.Domain` has zero package references; dependency
  direction is inward at every layer (verified via every `.csproj` and a
  `using`-statement sweep).
- **LOW** — `DPDP.Application` references `Microsoft.EntityFrameworkCore`
  directly, used only to type `DbSet<T>` in `IAppDbContext`. Deliberate,
  documented trade-off; the concrete `DpdpDbContext` stays in
  Infrastructure.
- **INFO** — No circular dependencies found across project or module
  boundaries. Compliance has zero references to Identity/Organisations;
  Identity's only cross-module touch is reading `Organisation.Id`/`Status`
  (the aggregate root's public shape, not internals) to validate a target
  tenant in `CreateUserCommand`.
- **LOW** — `ComplianceEndpoints.cs` (352 lines) is the largest
  hand-written backend file — purely declarative route wiring for 9
  sub-resources, not a God class in the OOP sense. Worth splitting by
  sub-resource if a 5th Compliance-adjacent resource is ever added.
- **INFO** — No repository-pattern layer exists on top of EF Core
  (`IAppDbContext` exposes `DbSet<T>` directly) — correctly avoids a
  redundant abstraction MASTER_PROMPT §23 warns against.
- **INFO** — No dead code, no `TODO`/`FIXME`/commented-out blocks, no
  template scaffolding found anywhere in the backend.
- **INFO** — No unused package dependencies in `Directory.Packages.props`
  or `package.json` (the two candidates that looked unused —
  `EFCore.Relational`/`Hosting.Abstractions` centrally pinned, and
  `@emotion/*` — are both real transitive/peer requirements).
- **INFO** — A few frontend features import another feature's `api.ts`
  directly (e.g. Departments → BusinessUnits for a dropdown). This calls
  each feature's only public surface (there's no barrel/index pattern),
  consistent with the pattern established since Module 2/3.

### 5.2 Database & performance (LOW/INFO)

- **LOW** — Search predicates across the codebase use
  `.Where(x => x.Name.ToUpper().Contains(term))`, which Postgres can't
  serve from a plain b-tree index. Irrelevant at current row counts
  (13–100s of rows); would only matter if a searched table (e.g. `Users`)
  reaches thousands of rows.
- **INFO** — Every table has a UUID PK; every entity that should be
  `ITenantScoped` is, with an explicit index backing its tenant filter;
  every business key with an obvious uniqueness need has a unique index
  (`Framework.Code`, `Control.ControlId`, `Requirement.Code`, `Role.Name`,
  `Permission.Key`, org-partitioned `User.NormalizedEmail`, etc.); FK
  columns get EF Core's automatic index by convention even without an
  explicit `HasIndex`.
- **INFO** — 4 migrations, each scoped to exactly one module; none edit or
  drop a prior migration's `HasData`; `dotnet ef migrations list` runs
  clean against the live dev database.
- **INFO** — `AsNoTracking()` is present on every real database-reading
  query handler across all three modules; the only two without it
  (`GetSystemInfoQueryHandler`, `GetAnswerStatusesQuery`) don't query the
  database at all.
- **INFO** — Delete behaviors consistently match documented product
  intent: `Restrict` where a parent shouldn't disappear while children
  reference it, `Cascade` only for true dependent/child-lifecycle records.
- **INFO** — Bare (non-paginated) list endpoints (`/roles`, `/permissions`,
  `/compliance/frameworks`, `/compliance/legal-references`,
  `/compliance/requirements`, `/compliance/control-categories`) are all
  bounded reference-data sets (8–35 rows) — correctly out of scope for
  pagination today.

### 5.3 Security (LOW/INFO)

- **INFO** — Every tenant-isolation-sensitive by-id handler across
  Identity/Organisations re-verified correct, including the four handlers
  the Module 3 bug originally touched (`GetOrganisationProfileQuery`,
  `UpdateOrganisationProfileCommand`, `GetOrganisationDashboardQuery`,
  `CreateOrganisationLocationCommand`) and all four Compliance
  ACTIVE-only-visibility queries (`GetControlsQuery`, `GetControlByIdQuery`,
  `GetQuestionsQuery`, `GetEvidenceRequirementsQuery`).
- **INFO** — Every one of the ~50 non-public Minimal API routes carries
  both `.RequireAuthorization()` and `.RequirePermission(...)`; all 22
  Compliance command handlers and both Role-permission-template commands
  have the `IsSuperAdministrator` guard.
- **INFO** — JWT validation has all four `Validate*` flags true, explicit
  HMAC-SHA256 (no `alg: none` risk), key sourced from config/env only;
  refresh tokens are 256-bit random, SHA-256-hashed at rest, rotated, with
  reuse-detection revoking the whole chain; password hashing uses ASP.NET
  Core Identity's PBKDF2-based hasher.
- **INFO** — CORS is dev-only permissive, allowlist-only elsewhere; no raw
  SQL string interpolation anywhere; no `[AllowAnonymous]` found; no
  hard-coded secrets anywhere in source; `.gitignore` correctly excludes
  `.env` variants; `GlobalExceptionHandler` hides 5xx details outside
  Development; login returns an identical error for "no such account" and
  "bad password" (no user-enumeration oracle).
- **INFO** — No file-upload endpoint exists yet (MASTER_PROMPT §7's
  file-security requirements have nothing to attach to until the Evidence
  module). No SSRF surface exists — `FrameworkVersion.SourceUrl`/
  `Control.SourceReference` are stored-only strings, never fetched
  server-side.
- **INFO** — `GET /api/v1/system/info` is deliberately public (app
  name/version/environment/server time only, no secrets) — acceptable as
  documented in its own doc comment.

### 5.4 Audit logging, validation, error handling (LOW/INFO)

- **INFO** — Audit logging coverage at the command-handler level is
  complete: 100% of mutating commands call `IAuditLogger` (the one real
  gap, failed logins, is fixed in §3.6). The `AuditLog` entity captures
  every field MASTER_PROMPT §8 requires.
- **INFO** — No DPDP Act section number, control ID, or legal obligation
  text is used as a literal in any conditional/branching logic outside
  `DpdpActSeedData.cs` and its EF configurations — clean pass on §9's
  "do not hard-code compliance logic" requirement.
- **INFO** — No scoring logic exists anywhere yet (Assessment Engine,
  Module 9, genuinely not started) — correct not-applicable state, not a
  defect.
- **LOW** — 15 identifier-only commands (`RevokeUserRoleCommand`,
  `ActivateControlCommand`, `DeleteOrganisationCommand`, etc.) have no
  FluentValidation validator class. All 15 take only `Guid`/`bool`
  parameters whose only real constraint is existence, already enforced by
  `NotFoundException` in the handler — a format validator would add
  nothing. Deliberate, consistent pattern, not a gap.
- **INFO** — Spot-checked 8 validators across all three modules:
  `MaximumLength` values consistently match DB column `HasMaxLength`
  exactly; enum membership via `Enum.TryParse`; password policy delegated
  to `IPasswordPolicy` rather than reimplemented per-validator.
- **INFO** — Zero log call sites reference password/token/secret/
  connection-string values. A single correlation ID verified to thread
  through Serilog's `LogContext`, `GlobalExceptionHandler`,
  `CustomizeProblemDetails`, and `AuditLogger` consistently — not just
  designed to, actually does.
- **INFO** — The DPDP Rules, 2025 empty-shell claim in
  `docs/COMPLIANCE_CONTENT_GOVERNANCE.md` re-verified accurate: the Rules
  `FrameworkVersion` row exists, zero `LegalReference`/`Requirement`/
  `Control` rows reference it.

### 5.5 API, frontend, tests, documentation (LOW/INFO)

- **LOW** — `POST /organisations/{id}/locations` is nested, but its
  update/delete are flat (`PUT /organisations/locations/{locationId}`).
  Cosmetic inconsistency; the tenant check still happens correctly via the
  location's own `ITenantScoped` filter.
- **LOW** — `POST /compliance/control-mappings` is the only create
  endpoint returning a bare `Guid` instead of a full DTO. Harmless (the
  mapping carries no additional server-computed fields) but breaks the
  otherwise-universal "create returns the created resource" convention.
- **LOW** — Query-key root style differs between older features (flat
  string, e.g. `["business-units", ...]`) and Compliance (namespaced,
  e.g. `["compliance", "controls", ...]`). Verified this does **not**
  cause a cache-invalidation bug — TanStack Query's prefix matching means
  both styles invalidate correctly today — but future modules should pick
  one convention.
- **LOW** — No direct unit tests instantiate a `*CommandValidator`/
  `*QueryValidator` in isolation (MASTER_PROMPT §18 lists "Validation
  Tests" as a distinct category); rules are only exercised indirectly via
  end-to-end API tests today.
- **LOW** — Zero `.test.tsx` files for the Compliance feature (7 pages + 1
  dialog) — expected for a module that just shipped.
- **LOW** — No dedicated "Security Tests" category or E2E test tooling
  exists; security properties are verified incidentally inside
  tenant-isolation/authorization tests. Reasonable for Phase 1/2 scope of
  a platform this size.
- **INFO** — Zero cases anywhere (Identity, Organisations, Compliance)
  where a Query or Command returns a raw `Domain.Modules.*` entity instead
  of a Dto/`PagedResult<Dto>` — the DTO boundary is fully enforced.
- **INFO** — Access token verified in-memory only (`useRef`, never
  `localStorage`/`sessionStorage`); the refresh-token-in-`localStorage`
  trade-off is explicitly named in a code comment in `AuthProvider.tsx`
  with its exact XSS implication — the code was already fine; only
  `docs/SECURITY.md` needed correcting (§3.1).

## 6. Recommended priority order for future work

1. M7 (pipeline-behavior refactor for the Super-Administrator guard) —
   before more write-heavy modules are added to Compliance-like global
   data.
2. M4/M5 (query splitting, `QuestionCount` projection) — before the
   control library grows meaningfully past its current 13-control seed.
3. M8 (missing docs) — a dedicated pass, not a side effect of future
   feature work.
4. M1/M2/M9 — at real-deployment time (reverse proxy, CSP for the SPA,
   startup config validation) rather than now, since the infrastructure
   they depend on (a committed `deployment/` reverse-proxy config) doesn't
   exist yet.
5. M3/M6 — low urgency; revisit if multi-admin concurrent writes to shared
   reference data become common, or if `SecurityStamp` is ever relied upon.

## 7. Files changed by this review

- `docs/SECURITY.md` — §4 and §6 corrected to match actual implementation.
- `src/Backend/DPDP.Api/Program.cs` — added `app.UseHsts()`.
- `src/Backend/DPDP.Api/Modules/Auth/AuthEndpoints.cs` — added rate
  limiting to `/auth/change-password`.
- `src/Backend/DPDP.Application/Modules/Identity/Commands/ChangePassword/ChangePasswordCommand.cs`
  — now revokes active refresh tokens on password change.
- `src/Backend/DPDP.Application/Modules/Identity/Commands/Login/LoginCommand.cs`
  — failed login attempts now also write to the unified audit log.
- `docs/ARCHITECTURE_REVIEW.md` — this document (new).

No database migration was required — none of the fixes changed the data
model.
