# DPDP-COMPASS — Project Plan

## 1. Purpose

DPDP-COMPASS is a DPDP Act, 2023 compliance management and continuous assessment
platform. It reports **Compliance Assessment Score, Readiness, Control
Effectiveness, Risk Level, Evidence Coverage, Open Findings, Remediation
Status, and Assessment Confidence** — it never asserts absolute legal
compliance. Final legal interpretation stays with the customer's
legal/privacy/compliance personnel; the platform is a management and
evidence tool, not a legal authority.

This document is the top-level plan. `ARCHITECTURE.md`, `MODULE_ROADMAP.md`,
`DATABASE.md`, `API.md` and `SECURITY.md` are its supporting detail and must
stay consistent with it and with each other.

## 2. Current Repository State (as inspected 2026-09-07)

```
dpdp-compass/
├── MASTER_PROMPT.md      # governing spec for this build
├── deployment/           # empty
├── docs/                 # empty (this plan set is the first content)
├── scripts/              # empty
├── src/                  # empty
└── tests/                # empty
```

- **Not yet a git repository.** Git must be initialized as the first Phase 1
  step (see §5).
- No backend, frontend, database, or CI/CD exists yet. This is a greenfield
  build from spec.

### Environment observed

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 10.0.111 | Matches MASTER_PROMPT's ASP.NET Core 10 requirement |
| Node.js | v22.22.0 | Suitable for Vite + React 18/19 |
| npm | 10.9.4 | |
| Docker | 29.5.3 | Available for Postgres/Redis/MinIO local dev |
| psql (native client) | not installed | Not required — Postgres will run in a container; `dotnet ef` and application code talk to it over the network |

Open items to confirm before/at Phase 1 execution: Docker Compose plugin
availability, outbound network access for NuGet/npm restore, and whether a
reverse proxy (nginx) is already provisioned on the host or needs installing.
These don't block planning and will be verified when Phase 1 is executed.

## 3. Guiding Principles (non-negotiable, from MASTER_PROMPT.md)

1. **Modular monolith**, not microservices. Clear module boundaries so a
   module *could* be extracted later, but no premature service split.
2. **Multi-tenancy from Day 1** — enforced at API, application/service,
   database/query, and authorization layers, not just the frontend.
3. **Permission-based RBAC**, never role-name checks scattered in code.
4. **Compliance logic lives in a versioned rules engine**, never hard-coded
   in controllers — the law changes; the code must not assume otherwise.
5. **Scoring is a configurable formula**, not a naive passed/total ratio.
6. **Data discovery stores metadata and masked samples**, not bulk copies of
   customer personal data.
7. **AI is an assistant, never an authority** — every AI output is labelled
   "AI-generated recommendation — human review required" and can never by
   itself mark a control compliant.
8. **No overengineering** — no speculative microservices, event buses,
   Kubernetes, or abstractions the current phase doesn't need.
9. **Module-by-module development** following the 13-step loop in
   MASTER_PROMPT §22 (objective → entities → tables → APIs → UI → security →
   dependencies → tests → implement → test → build → fix → document), with a
   completion report per module.
10. **Nothing proceeds past a phase without an explicit approval gate.**

## 4. Phase Map (from MASTER_PROMPT §25)

| Phase | Modules | Status |
|---|---|---|
| 1 | Project Foundation, Database, Identity, Organisation Management, RBAC, Audit Logging | **Proposed below — awaiting approval** |
| 2 | Compliance Framework, DPDP Control Library, Assessment Engine, Risk Engine, Findings, Remediation, Evidence | Not started |
| 3 | Dashboard, Reports, Asset Inventory | Not started |
| 4 | Data Discovery, Data Classification, Data Inventory, Processing Activities, Data Flow Mapping | Not started |
| 5 | Privacy Notices, Consent, Data Principal Rights, Grievance, Retention, Deletion | Not started |
| 6 | Vendor Management, Processor Management, DPIA, Incident/Breach Management | Not started |
| 7 | Notifications, Integrations, AI Assistant, Continuous Monitoring | Not started |

Full module-to-entity-to-dependency detail is in `MODULE_ROADMAP.md`.

## 5. Phase 1 Proposal — Project Foundation

Phase 1 delivers a running, empty-but-correct skeleton: solution structure,
local infrastructure, authentication, organisations (tenants), RBAC, and
audit logging — nothing DPDP-specific yet. Every later module hangs off this
skeleton, so it is the highest-leverage place to get architecture right
before any application code is written (per the current task's constraint).

**In scope:**

1. Git initialization (`.gitignore`, initial commit), repo hygiene
   (`.env.example`, `README.md`).
2. Solution/folder scaffold matching `ARCHITECTURE.md` §2:
   `DPDP.Domain`, `DPDP.Application`, `DPDP.Infrastructure`, `DPDP.Api`,
   `dpdp-web`, `DPDP.UnitTests`, `DPDP.IntegrationTests`, `DPDP.ApiTests`.
3. `docker-compose.yml` for local dev: PostgreSQL, Redis, MinIO (S3-compatible
   storage stub for later Evidence work), pgAdmin optional.
4. EF Core `DpdpDbContext`, first migration covering: `organisations`,
   `users`, `roles`, `permissions`, `role_permissions`, `user_roles`,
   `refresh_tokens`, `audit_logs` (full schema in `DATABASE.md`).
5. Identity module: registration is admin-driven (no public self-signup for
   an enterprise compliance tool), login, JWT access + rotating refresh
   token, logout, password hashing via ASP.NET Core Identity, account
   lockout on repeated failed logins, MFA-ready fields (not enforced yet).
6. Organisation module: CRUD for organisations (tenants), organisation
   status (active/suspended), one org per non-Super-Admin user.
7. RBAC: permission catalogue seeded, default roles (Super Administrator,
   Organisation Administrator, Privacy Officer, Compliance Officer, Security
   Officer, IT Administrator, Department Owner, Auditor,
   Management/Executive, Read Only User) seeded with a starting permission
   set, custom `IAuthorizationHandler`/policy provider resolving
   `permission.action` strings.
8. Audit logging: `IAuditLogger` service + MediatR pipeline behavior that
   records login/logout/failed-login/user changes/role changes/org changes
   per MASTER_PROMPT §8; audit write path isolated from normal app
   read/write permissions (append-only).
9. Cross-cutting middleware: global exception handler → ProblemDetails,
   correlation ID, security headers, CORS allowlist, rate limiting on auth
   endpoints, Serilog structured logging, health checks
   (`/health`, `/health/ready`).
10. Frontend skeleton: Vite + React + TS + MUI shell with login page,
    protected route wrapper, TanStack Query client, and the navigation shell
    from MASTER_PROMPT §16 (routes stubbed, most showing "coming soon").
11. Tests: unit tests for password/JWT/permission logic, integration tests
    for auth + tenant isolation (a User in Org A must get 403/empty results
    against Org B data), API tests for the Phase 1 endpoint surface.

**Explicitly out of scope for Phase 1:** anything DPDP-specific (frameworks,
controls, assessments, risk, findings, evidence, data discovery, consent,
etc.) — those are Phase 2+.

**Phase 1 quality gate** (must all pass before Phase 2 starts, per
MASTER_PROMPT §24): backend builds; frontend builds; migration applies
cleanly; auth API works end-to-end; login UI works; input validation via
FluentValidation; authorization via permission policies; tenant isolation
verified by test; audit logging fires on the listed events; unit +
integration tests pass; no critical security issue; docs updated
(`docs/modules/identity.md`, `docs/modules/organisations.md`,
`docs/modules/rbac.md`, `docs/modules/audit.md`); README updated; no
secrets committed.

## 6. Approval Gates

Per MASTER_PROMPT §27, this plan stops here. **No application code will be
written until this plan set is reviewed and Phase 1 is explicitly
approved.** Once approved, Phase 1 will itself be executed module-by-module
(Foundation → Database → Identity → Organisation → RBAC → Audit Logging),
each with its own completion report (implementation, files
touched, DB changes, API endpoints, UI changes, tests, security notes,
commands run, build/test results, known issues, recommended next step), and
Phase 2 will not start until Phase 1's quality gate is confirmed passed.

## 7. Assumptions & Risks

- **.NET 10 / ASP.NET Core 10** is used per spec; as a very recent major
  version, package ecosystem maturity (e.g. some third-party libraries) will
  be checked during Phase 1 execution and flagged if a pin to a compatible
  version is needed.
- **No legal review has occurred.** Per MASTER_PROMPT §26, no DPDP Act
  sections/rules will be invented; the Phase 2 Compliance Framework module
  will store framework content but treat it as *draft/unreviewed* until
  approved by a legal/privacy professional — this is a Phase 2 concern,
  flagged here so it isn't lost.
- **Antivirus scanning for evidence uploads** (MASTER_PROMPT §7) is an
  architectural placeholder (an `IAvScanner` interface) in Phase 1/2; a real
  ClamAV or cloud AV integration is deferred until the Evidence module
  (Phase 2) is implemented, and will be called out again then.
- **OIDC/external IdP** is architected for but not wired to a real provider
  in Phase 1 — first-party JWT auth ships first; OIDC is additive later.
