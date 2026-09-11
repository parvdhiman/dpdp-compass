# DPDP-COMPASS — Security

This platform manages an organisation's compliance posture, findings, risk
data, and (from Phase 4 onward) metadata about personal data processing. It
is treated as a security-sensitive enterprise application from Phase 1, not
retrofitted later.

## 1. Threat Model Summary

- **Multi-tenant SaaS**: the primary threat is cross-tenant data exposure —
  a bug that lets Organisation A see Organisation B's assessments, findings,
  or users. Mitigated by the four-layer tenant isolation in
  `ARCHITECTURE.md` §4, verified by dedicated integration tests every
  module ships with (MASTER_PROMPT §18).
- **Privileged insider / compromised admin account**: Super Administrator
  and Organisation Administrator actions are the highest-blast-radius
  operations in the system; every one is audit-logged (§6 below) and
  permission-gated individually rather than bundled behind a coarse "is
  admin" check.
- **Compromised credentials**: mitigated by password hashing, lockout,
  MFA-ready fields, short-lived access tokens, and rotating refresh tokens
  with revocation.
- **Uploaded file abuse** (from Phase 2 Evidence onward): mitigated by type
  allowlisting, size limits, storage isolation, and an AV-scan hook — see
  §8.

## 2. Authentication

- ASP.NET Core Identity for user store and password hashing (PBKDF2 via
  Identity's default hasher at Phase 1; documented here as the concrete
  choice so it isn't silently assumed — an upgrade to Argon2id is a
  drop-in `IPasswordHasher<User>` replacement if a future security review
  calls for it, and is not blocked by anything in this design).
- **JWT access tokens**, short-lived (15 minutes), signed with a key from
  configuration/secret store — never hard-coded (MASTER_PROMPT §7/§21).
- **Rotating refresh tokens**: stored server-side only as a hash
  (`refresh_tokens.token_hash`), never the raw token; each use issues a new
  token and revokes the old one (`replaced_by_token_id` chain), so a stolen-
  and-reused old refresh token is detectable and the chain can be revoked
  entirely on reuse detection.
- **MFA-ready**: `users.mfa_enabled` / `mfa_secret` (encrypted at rest)
  exist from the Phase 1 schema; enforcement UI/flow is not required for
  Phase 1 to pass its gate but the data model doesn't need a breaking
  change to add it later.
- **OIDC-ready**: the auth module is structured so an external IdP
  (Azure AD / Okta / etc.) can be added as an additional authentication
  scheme without changing the authorization model (which runs entirely on
  permission claims, not on how the user authenticated).
- **Account lockout**: `failed_login_count` / `lockout_end` enforced via
  ASP.NET Core Identity's lockout provider; every attempt (success or
  failure) is recorded in `login_history`.

## 3. Authorization

- **Permission-based**, never `[Authorize(Roles = "...")]` — see
  `ARCHITECTURE.md` §5. Deny-by-default: an endpoint with no explicit
  permission requirement is a bug, not an "open" endpoint; a Roslyn
  analyzer/test asserting every endpoint declares a permission (or is
  explicitly listed as public: login, refresh, health) is part of the
  Phase 1 API test suite.
- **Tenant + permission are checked independently** (`ARCHITECTURE.md`
  §4) — holding a permission never implies access to another tenant's row.

## 4. Transport & Headers

**Corrected 2026-09-09 (architecture review) — this section previously
described the pre-implementation design sketch and had drifted out of sync
with what Module 2 actually built; see §12 below, which was and remains
accurate. The bullets below now describe the real, current implementation.**

- HTTPS only in every environment above local dev, via
  `app.UseHttpsRedirection()`. **HSTS is not yet enabled** (`app.UseHsts()`
  is never called, no `Strict-Transport-Security` header is set) — tracked
  as a HIGH finding in `docs/ARCHITECTURE_REVIEW.md`; add before any
  production deployment.
- Security headers actually set on every API response
  (`SecurityHeadersMiddleware`): `X-Content-Type-Options: nosniff`,
  `X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin`,
  and `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'`
  — deliberately maximally restrictive rather than "scoped to the
  frontend's origin," since this middleware runs on the JSON API only,
  which serves no HTML of its own. A separate CSP for the frontend's own
  served HTML page belongs in the static-hosting/reverse-proxy config once
  one is committed under `deployment/` (not yet present).
- **CORS**: explicit origin allowlist from configuration, no `*` wildcard,
  credentials-aware only where required. Verified accurate.
- **Tokens**: the access token is held client-side in memory only (never
  `localStorage`/`sessionStorage`/a cookie). The refresh token is stored in
  `localStorage`, **not** an `HttpOnly` cookie — this is the opposite of
  this section's original design sketch, changed deliberately during
  Module 2 because the frontend and API can be deployed on different
  origins/ports, which a cookie-based approach would complicate. No CSRF
  antiforgery mechanism exists because there is no cookie-based
  authentication flow to protect. See §12 for the full trade-off
  discussion (this was already documented correctly there — only this
  section was out of date).
- **Rate limiting**: ASP.NET Core's built-in fixed-window rate limiter on
  `/auth/login`, `/auth/refresh`, and `/auth/forgot-password`, partitioned
  **per client IP only** (there is no separate per-account dimension).
  Known limitation: partitioning by `RemoteIpAddress` will collapse to one
  shared bucket once deployed behind a reverse proxy unless
  `UseForwardedHeaders` with a trusted-proxy list is configured first —
  tracked as a MEDIUM finding in `docs/ARCHITECTURE_REVIEW.md`.

## 5. Input/Output Handling

- **Validation**: FluentValidation on every command/query at the
  Application layer (`ARCHITECTURE.md` §3); API-layer model binding is a
  first pass, not a substitute.
- **SQL injection**: EF Core parameterized queries exclusively; no raw SQL
  string concatenation anywhere. Any future raw SQL (e.g. for a reporting
  query) must use parameterized `FromSqlInterpolated`, never
  `FromSqlRaw` with interpolated user input.
- **XSS**: React escapes by default; the API never returns
  pre-rendered HTML from user input. Rich-text fields (if any, later
  phases) are sanitized server-side before storage, not just at render.
- **SSRF**: no server-side code fetches a URL supplied by a user without
  validation against an explicit allowlist; this becomes concretely
  relevant starting with Integrations (Phase 7) and is called out again in
  that module's design doc.
- **Path traversal**: file storage keys are server-generated GUIDs, never
  derived from user-supplied filenames; the original filename is stored as
  metadata only, never used to construct a filesystem/object-storage path.

## 6. Audit Logging (tamper-resistance)

**Corrected 2026-09-09 (architecture review)** — the bullet below
previously claimed a database-enforced append-only grant exists. It does
not: there is no migration, SQL script, or deployment file anywhere in the
repository that creates a restricted Postgres role or `REVOKE`s
`UPDATE`/`DELETE` on `audit_logs`. The single configured application
database user owns the schema (it ran the migrations) and therefore has
full `UPDATE`/`DELETE` rights on this table today. This was a HIGH finding
in `docs/ARCHITECTURE_REVIEW.md` — a security document asserting a control
that isn't implemented is worse than one that honestly notes the gap.

- `audit_logs` is **append-only by application convention only, not yet by
  database enforcement**: `IAuditLogger`'s only write path is `INSERT`
  (verified — no code anywhere calls `.Update(...)` or `.Remove(...)` on
  `db.AuditLogs`), but nothing at the database level prevents a direct
  `UPDATE`/`DELETE` against the table by whatever role the application
  connects as. Genuine tamper-resistance requires a dedicated,
  narrowly-privileged Postgres role (`INSERT`/`SELECT` only on this table)
  provisioned outside EF Core migrations (the migration-running role
  necessarily has DDL rights) — this is recommended future work, not yet
  implemented, and intentionally not added by this review (see
  `docs/ARCHITECTURE_REVIEW.md` — implementing new database-security
  infrastructure was out of scope for a documentation-and-fixes-only
  audit pass).
- Every record carries the actor, tenant, action, entity type/id, before/
  after values (JSON), IP, user agent, and correlation id, per §8 of
  MASTER_PROMPT.
- Phase 1 audit coverage: login, logout, failed login, user creation/
  modification, role changes, organisation changes. Each later module adds
  its own events to this same sink (assessment/evidence/finding/risk/
  vendor/consent/privacy-request/incident/configuration actions) — the sink
  and its guarantees don't change per module.

## 7. Secrets Management

- No secret (DB credentials, JWT signing key, SMTP credentials, AI provider
  keys, object storage credentials) is ever committed. `.env.example`
  lists required variable names with placeholder/blank values only;
  real `.env` files are git-ignored.
- Local dev reads secrets from `.env`/`appsettings.Development.json`
  (git-ignored); production reads from environment variables injected by
  the deployment mechanism (systemd `EnvironmentFile=` or Docker secrets),
  never from a committed `appsettings.Production.json`.
- Connection strings, JWT keys, and API keys are never logged; the global
  exception handler and Serilog configuration are both checked (as part of
  the Phase 1 quality gate) to confirm no configuration object is ever
  passed whole into a log call.

## 8. File Upload Security (designed now, enforced starting Phase 2 Evidence)

- Allowlisted content types and extensions only (double-checked by magic-
  byte sniffing, not just the client-supplied MIME type/extension).
- Maximum upload size enforced at both the reverse proxy and the
  application layer.
- Files are stored in object storage (MinIO/S3/Azure Blob via
  `IObjectStorage`), never on the web server's local filesystem, and never
  under a user-influenced path — see path traversal note in §5.
- An `IAvScanner` interface is defined in Foundation as an architectural
  placeholder; a real scanner (e.g. ClamAV) is wired in when Evidence
  (Phase 2) actually accepts uploads — files are not made available for
  download until they pass the scan.

## 9. Secure Error Handling

- Global exception-handling middleware maps every unhandled exception to a
  generic RFC 7807 body in Production (`API.md` §1); the full exception
  (message + stack trace) is logged server-side with the correlation id so
  it's still diagnosable, but is never returned to the client.
- Database connection strings, JWT secrets, and internal stack traces are
  explicitly on the "never expose" list from MASTER_PROMPT §7 and are
  covered by the same middleware, not left to each endpoint to handle
  individually.

## 10. Compliance-Framing Constraint (product-level, security-adjacent)

Per MASTER_PROMPT's opening constraint, no part of the system — including
error messages, dashboard copy, report templates, or AI output — may state
or imply an organisation is "legally 100% compliant." This is enforced as a
content/design rule reviewed at each module's completion report, and
concretely as a required disclaimer wrapper on all AI-generated output
("AI-generated recommendation — human review required," MASTER_PROMPT
§13) and on any exported report.

## 11. Testing Coverage for the Above

Per MASTER_PROMPT §18, every module's test suite includes — in addition to
functional unit/integration/API tests — explicit **Authorization Tests**,
**Tenant Isolation Tests**, **Validation Tests**, and **Security Tests**.
For Phase 1 specifically this means: a user from Organisation A cannot read
or write Organisation B's data via any Phase 1 endpoint (verified for
users, roles, audit logs); an unauthenticated request to any non-public
endpoint is rejected; a request lacking the required permission is
rejected even when authenticated; lockout triggers after the configured
number of failed logins; refresh token reuse after rotation is detected and
the chain is revoked.

## 12. Module 2 (Identity, Authentication & RBAC) — As Implemented

- **Password policy** (`PasswordPolicy` in `DPDP.Application`): minimum 12
  characters, at least one uppercase, one lowercase, one digit, one
  special character, and must not contain the local part of the account's
  own email. Enforced on every password-setting path (admin-created
  accounts, self-service change, reset) via FluentValidation validators
  that all reference the same `IPasswordPolicy`, and again by
  `IdentityBootstrapper` for the bootstrap Super Administrator's password.
- **Password hashing:** `Microsoft.AspNetCore.Identity.PasswordHasher<T>`
  (PBKDF2) used standalone as a hashing utility, per §2's original design —
  this project doesn't use ASP.NET Core Identity's full
  UserManager/SignInManager system, only this one hashing class.
- **JWT claims:** `sub`/`NameIdentifier` (user id), `email`, `name`,
  `org_id` (omitted claim, not empty string, when null — Super
  Administrator), `is_super_admin`, one `role` claim per assigned role
  (display only), one `permission` claim per resolved permission key
  (authorization checks use only these). Signed HMAC-SHA256, key from
  `Jwt:SigningKey` (user-secrets/environment only, never appsettings.json),
  15-minute default expiry (`Jwt:AccessTokenMinutes`).
- **Refresh tokens:** opaque random 256-bit tokens; only a SHA-256 hash is
  ever persisted. Rotated on every use; reuse of an already-rotated token
  revokes every active token for that user (compromise-assumed response),
  logged via `auth.refresh_token_reuse_detected`.
- **Refresh token storage on the frontend is a deliberate, documented
  trade-off, not the original cookie-based sketch in §4**: it's kept in
  `localStorage`, not an HttpOnly cookie, because the frontend and API can
  be deployed on entirely different origins/ports and a cookie-based
  approach would need same-site domains or `SameSite=None; Secure` plus a
  CORS/credentials story that isn't worth the complexity yet. The access
  token itself stays in memory only, never persisted. This trade-off means
  an XSS bug could read the refresh token — the mitigation is the same
  CSP/React-escaping posture as before, not a cookie. Revisit if/when
  frontend and API share a parent domain in production.
- **Account lockout:** `AccountSecurity:MaxFailedLoginAttempts` (default 5)
  failed attempts locks the account for `AccountSecurity:LockoutMinutes`
  (default 15) — `423 Locked`, not `401`, so the client can show a
  specific message; this is a deliberate, minor, accepted enumeration
  trade-off (revealing "this account is locked" vs. "invalid credentials")
  that MASTER_PROMPT's explicit lockout requirement implies is worth it.
- **Login/forgot-password rate limiting** is per-client-IP (a named but
  *unpartitioned* limiter would share one bucket across every caller —
  caught during Module 2 manual testing, see the completion report) and
  its limit is configurable (`RateLimiting:Auth`), strict by default and
  relaxed only in `appsettings.Development.json` — a fast automated test
  suite hitting `/auth/*` repeatedly needs the relaxed limit, since
  `WebApplicationFactory` requests all share one loopback "IP".
- **Password reset delivery:** there is no email module yet (Notifications
  is Phase 7), so `POST /auth/forgot-password` returns the raw reset token
  in its response body **only when `ASPNETCORE_ENVIRONMENT=Development`**
  (`ForgotPasswordResponse.DevOnlyResetToken`); Staging/Production get
  `null` always. The token itself is never written to any log — see §6's
  original "never log tokens" rule, which this was designed against
  explicitly.
- **`AuditableEntitySaveChangesInterceptor`** stamps `created_at/by` and
  `updated_at/by` automatically on every `SaveChanges` call, so no command
  handler has to remember to do it — registered **Scoped**, not Singleton
  (it depends on the per-request `ICurrentUserContext`; the DI container's
  own startup validation caught this when it was first registered as a
  Singleton — see the Module 2 completion report).
