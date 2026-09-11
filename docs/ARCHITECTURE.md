# DPDP-COMPASS — Architecture

## 1. Style: Clean Architecture inside a Modular Monolith

One deployable backend process, one deployable frontend, internally
partitioned into **vertical feature modules** that each span all four Clean
Architecture layers. This satisfies both structural directives in
MASTER_PROMPT (§3's layered `DPDP.Domain/Application/Infrastructure/Api`
split, and §4's `Modules/Identity, Organisations, Assets, ...` list) by
making "module" the *vertical* cut and "layer" the *horizontal* cut of the
same solution — no God Service, no premature microservices.

```
Layer (horizontal)          →  Domain → Application → Infrastructure → Api
Module (vertical)           ↓
  Identity
  Organisations
  Assets
  Frameworks
  Controls
  Assessments
  Risks
  Findings
  Remediation
  Evidence
  DataDiscovery
  DataInventory
  ProcessingActivities
  DataFlows
  Consent
  PrivacyNotices
  DataPrincipalRights
  Grievances
  Retention
  Deletion
  Vendors
  DPIA
  Incidents
  Audit
  Reports
  Notifications
  Integrations
  AI
```

A module may only depend on another module through its **Application-layer
contracts** (interfaces/DTOs), never by reaching into another module's
Domain internals or Infrastructure directly. This is the seam that would let
a module be extracted into its own service later without a rewrite — but no
module is extracted now (MASTER_PROMPT §23).

## 2. Solution / Folder Structure

```
src/
  Backend/
    DPDP.Domain/
      Common/                     # Entity, AggregateRoot, ITenantScoped, ValueObjects
      Modules/
        Identity/Entities/            (User, Role, Permission, RolePermission, UserRole, RefreshToken)
        Organisations/Entities/       (Organisation)
        Audit/Entities/                (AuditLog)
        <Module>/Entities/...          # one subfolder per module in §1, added as each is built

    DPDP.Application/
      Common/
        Behaviors/                  # ValidationBehavior, LoggingBehavior, AuditBehavior, TenantGuardBehavior
        Interfaces/                 # ICurrentUser, ICurrentTenant, IAppDbContext, IDateTimeProvider, IAuditLogger
      Modules/
        Identity/
          Commands/                 # LoginCommand, RefreshTokenCommand, ChangePasswordCommand...
          Queries/                  # GetCurrentUserQuery...
          DTOs/
          Validators/
        Organisations/{Commands,Queries,DTOs,Validators}/
        Audit/{Queries,DTOs}/
        <Module>/...

    DPDP.Infrastructure/
      Persistence/
        DpdpDbContext.cs
        Configurations/            # IEntityTypeConfiguration<T> per entity, grouped by module folder
        Migrations/
        Interceptors/              # audit/soft-delete/tenant-stamping SaveChanges interceptor
      Identity/                    # ASP.NET Core Identity store wiring, JWT issuing, password hashing
      Security/                    # PermissionAuthorizationHandler, PermissionPolicyProvider
      Storage/                     # IObjectStorage + S3/MinIO/Azure Blob implementations (from Phase 2 Evidence on)
      Caching/                     # Redis-backed ICacheService
      Logging/                     # Serilog configuration
      Services/                    # module service implementations bound to Application interfaces

    DPDP.Api/
      Modules/
        Identity/Endpoints/        # or Controllers — see §7
        Organisations/Endpoints/
        Audit/Endpoints/
        <Module>/Endpoints/
      Middleware/                  # ExceptionHandling, CorrelationId, SecurityHeaders
      Filters/
      Program.cs

  Frontend/
    dpdp-web/
      src/
        app/                       # router, query client, theme, layout shell
        features/
          auth/
          organisations/
          <module>/                # one folder per module, added as each ships
        components/                # shared, generic UI components
        lib/                       # api client, zod schemas, auth context

Tests/
  DPDP.UnitTests/           # per module: Domain rules, scoring, validators
  DPDP.IntegrationTests/    # per module: DbContext-backed, Testcontainers Postgres, tenant isolation
  DPDP.ApiTests/            # per module: WebApplicationFactory, full HTTP round trip incl. auth

docs/
  PROJECT_PLAN.md, ARCHITECTURE.md, MODULE_ROADMAP.md, DATABASE.md, API.md, SECURITY.md
  modules/<module>.md       # one per module, added as each ships

deployment/
  docker-compose.yml, docker-compose.override.yml (dev), nginx/, systemd/

scripts/
  db-migrate.sh, seed.sh, dev-up.sh, etc.
```

Rule: **frontend and backend never share business logic.** The frontend
holds Zod schemas that mirror API contracts for client-side validation
convenience only; the API is the source of truth and re-validates
everything server-side (FluentValidation).

## 3. Cross-Cutting Concerns

- **Validation:** FluentValidation validators per command/query, run by a
  MediatR `ValidationBehavior` before the handler executes. API-layer model
  binding errors and FluentValidation failures both surface as RFC 7807
  ProblemDetails (§ see `API.md`).
- **Error handling:** one global exception-handling middleware maps
  exceptions → ProblemDetails; never leaks stack traces or connection
  strings in any non-Development environment (MASTER_PROMPT §7).
- **Correlation:** every request gets/keeps an `X-Correlation-Id`, flowed
  into Serilog scope and into `AuditLog.correlation_id`.
- **Audit:** a MediatR `AuditBehavior` + an explicit `IAuditLogger` for
  events that aren't naturally commands (e.g. login, failed login). See
  `SECURITY.md` §Audit for tamper-resistance approach.
- **Background jobs:** Hangfire, backed by Postgres storage (avoids adding
  another moving part) unless a Phase's job volume later justifies Redis
  storage.

## 4. Multi-Tenancy

- Tenant = `Organisation`. Every tenant-scoped entity carries a non-null
  `tenant_id` (= `organisation_id`) FK, enforced at four layers as
  MASTER_PROMPT §5 requires:
  1. **API:** the JWT carries an `org_id` claim; endpoints never accept a
     caller-supplied tenant id for scoping — it is always taken from the
     authenticated principal (an explicit `organisationId` route/query
     param is permitted only for Super Administrator cross-tenant screens,
     and is itself permission-gated).
  2. **Application/service:** an `ICurrentTenant` service resolves the
     active tenant once per request; handlers depend on it rather than
     reading claims directly, so tenant scoping is testable and can't be
     silently bypassed by a handler that forgets to filter.
  3. **Database/query:** EF Core **global query filter** on every
     `ITenantScoped` entity (`HasQueryFilter(e => e.TenantId ==
     _currentTenant.Id)`), applied in `DpdpDbContext`. A `SaveChanges`
     interceptor stamps `tenant_id` on insert and rejects an update that
     would move a row to a different tenant.
  4. **Authorization:** permission checks and tenant checks are independent
     — a user can hold `assessment.read` and still be denied if the target
     row's tenant doesn't match theirs; the authorization handler checks
     both.
  - Super Administrator is the only role allowed to operate without a fixed
    tenant filter (cross-tenant admin screens), and every such access is
    audit-logged with the target tenant id explicit.
  - Row-Level Security (Postgres RLS) is noted as a **future hardening
    option**, not required for Phase 1 — the four-layer approach above
    already satisfies "no reliance on frontend filtering," and RLS adds
    operational complexity (session variables per connection) that isn't
    justified until a specific defense-in-depth need is identified.

## 5. RBAC

Permission-based, not role-name-based, per MASTER_PROMPT §6.

- `Permission` is a static catalogue row (e.g. `organisation.read`,
  `assessment.approve`, `evidence.upload`) — see `DATABASE.md` for the
  seed list.
- `Role` is a named, tenant-agnostic template (Super Administrator,
  Organisation Administrator, Privacy Officer, Compliance Officer, Security
  Officer, IT Administrator, Department Owner, Auditor,
  Management/Executive, Read Only User) with a `RolePermission` mapping.
- `UserRole` assigns one or more roles to a user **within their
  organisation** (role assignment itself is tenant-scoped, except Super
  Administrator which is global).
- At request time, permissions are resolved (role → permissions) into JWT
  claims at token issuance (kept short — a role/permission change requires
  the affected user's tokens to be refreshed/revoked; refresh path checks
  current DB state, so within one refresh cycle, revoked permissions take
  effect).
- Enforcement is via a custom `IAuthorizationRequirement`/
  `AuthorizationHandler` and a dynamic `IAuthorizationPolicyProvider` that
  turns an attribute like `[RequirePermission("evidence.upload")]` into a
  policy check — no `[Authorize(Roles = "...")]` anywhere in the codebase.

## 6. Compliance Engine (framework/control library implemented in Module 4; assessment engine still pending)

Domain graph, versioned end-to-end so "the law changes" never means a code
change:

```
Framework (versioned, e.g. "DPDP", version "2025")
  → Section (law/regulation section reference)
    → Requirement
      → Control (e.g. DPDP-CONTROL-001)
        → Assessment Question (with answer domain: YES/PARTIAL/NO/NOT_APPLICABLE)
          → Evidence Requirement
          → Risk (severity)
          → Remediation (guidance)
```

Every node above carries `source`, `publication_date`, `version`,
`section_reference`, `effective_date`, `applicability`, and
`interpretation_notes` (MASTER_PROMPT §26), and a `review_status`
(`draft` / `legal_reviewed` / `approved`) so unreviewed content can never be
presented as authoritative. This is designed now (so Phase 1's schema
doesn't need a breaking change later) but populated starting Phase 2.

Control status domain: `PASS / PARTIAL / FAIL / NOT_APPLICABLE /
NOT_ASSESSED / NEEDS_REVIEW` (§10). Scoring is a pluggable
`IComplianceScoringStrategy` combining control weight, risk severity,
evidence confidence, and assessment coverage — never a bare passed/total
ratio (§11); the concrete formula is configuration, not code, and is
documented alongside its implementation when Phase 2 ships it.

**As implemented in Module 4** (see §14 for the full write-up): the graph
above is now real —
`Framework → FrameworkVersion → LegalReference → Requirement →(ControlMapping)→
Control → AssessmentQuestion → EvidenceRequirement`, plus a standalone
`ControlCategory` and a `ContentReviewStatus` (`DRAFT` / `LEGAL_REVIEWED` /
`APPROVED`) on every legal-content node, exactly as this section
anticipated. The `AnswerStatus` enum (`PASS / PARTIAL / FAIL /
NOT_APPLICABLE / NOT_ASSESSED / NEEDS_REVIEW`) is defined in the Domain
layer per this section's scope, but nothing persists an answer yet — that
is the Assessment Engine module, still pending.

## 7. API Layer Approach

REST, versioned under `/api/v1`. Minimal API endpoint groups (per module,
under `DPDP.Api/Modules/<Module>/Endpoints`) are preferred over MVC
controllers for a modular monolith of this shape — they map 1:1 onto the
vertical module folders and keep routing colocated with the module instead
of in a shared `Controllers/` folder that invites cross-module coupling.
DTOs only; entities never cross the API boundary. Full conventions in
`API.md`.

## 8. Frontend Architecture

React + TypeScript + Vite, MUI as the component library, TanStack Query for
server state, React Hook Form + Zod for forms, React Router for navigation.
Navigation matches MASTER_PROMPT §16 (Dashboard, Organisation, Assets, Data
Discovery, Data Inventory, Processing Activities, Data Flows, Compliance
[Assessments/Controls/Findings/Remediation], Privacy [Consent/Notices/DPR/
Grievances/Retention/Deletion], Third Parties [Vendors/Processors], Risk
[Register/DPIA], Incidents, Audit, Evidence, Reports, Administration
[Users/Roles/Settings], AI Assistant). Unbuilt sections render a permission-
aware "not yet available" placeholder rather than a broken link, so the full
navigation shell can ship in Phase 1 without implying features exist early.

## 9. Deployment Architecture

- **Local/dev:** `docker-compose.yml` — Postgres, Redis, MinIO, and (Phase 4+)
  OpenSearch, plus the API and web containers with hot reload.
- **Production (primary target: Ubuntu Linux):** Docker Compose or systemd-
  managed services behind an nginx reverse proxy terminating HTTPS; object
  storage via MinIO (self-hosted) or AWS S3/Azure Blob through the
  `IObjectStorage` abstraction — swappable by configuration, not code.
- **Future:** Kubernetes manifests, only if/when operational scale actually
  requires it (§23 — not built speculatively now).

## 10. Technology Stack Summary

| Layer | Choice |
|---|---|
| Backend runtime | ASP.NET Core 10 Web API, C# |
| ORM | Entity Framework Core |
| Database | PostgreSQL |
| Cache | Redis |
| Search (Phase 4+) | OpenSearch |
| Background jobs | Hangfire (Postgres storage) |
| Object storage | S3-compatible via abstraction (MinIO / AWS S3 / Azure Blob) |
| AuthN | ASP.NET Core Identity, JWT access + rotating refresh tokens, OIDC-ready |
| Validation | FluentValidation |
| Mediator | MediatR |
| Logging | Serilog (structured, sinks: console + file, correlation-id enriched) |
| API docs | OpenAPI/Swagger |
| Frontend | React, TypeScript, Vite, MUI, TanStack Query, React Hook Form, Zod, React Router |
| Containerization | Docker, Docker Compose |
| Reverse proxy | nginx |
| Process mgmt (prod) | systemd |

This stack matches MASTER_PROMPT §2 exactly; no substitutions proposed.

## 11. Module 1 (Project Foundation) — As Implemented

The design above held up unchanged through implementation. This section
records the concrete decisions made while realizing it, so later modules
build on what actually exists rather than what was planned in the abstract.

- **Solution file:** `dotnet new sln` on the installed .NET 10 SDK produces
  the new XML `DPDP.slnx` format, not `.sln` — referenced as such in
  scripts and CI.
- **Central Package Management:** `Directory.Packages.props` at the repo
  root pins every NuGet package version, including transitive ones
  (`CentralPackageTransitivePinningEnabled`). This was added after the
  first build hit a real conflict — `Npgsql.EntityFrameworkCore.PostgreSQL`
  floors `Microsoft.EntityFrameworkCore.Relational` at a lower patch than
  the version this solution otherwise resolves to, which silently
  downgraded that assembly in the test projects' dependency graph. Every
  `<PackageReference>` across the solution is now version-less; versions
  live in one file only.
- **Endpoints are Minimal API route groups**, not controllers — confirms
  §7 below the abstraction. `DPDP.Api/Modules/<Module>/*Endpoints.cs`
  exposes a `Map<Module>Endpoints(this IEndpointRouteBuilder)` extension,
  called once from `Program.cs`.
- **API versioning** uses `Asp.Versioning.Http` with
  `UrlSegmentApiVersionReader` (`/api/v{version}/...`) and
  `Asp.Versioning.Mvc.ApiExplorer` to generate one Swagger document per
  version via a custom `IConfigureOptions<SwaggerGenOptions>`
  (`DPDP.Api/OpenApi/ConfigureSwaggerOptions.cs`) — adding v2 later means
  adding the version to the version set, not touching Swagger setup.
- **Global exception handling** is the .NET 8+ `IExceptionHandler` pattern
  (`DPDP.Api/ExceptionHandling/GlobalExceptionHandler.cs`), not custom
  try/catch middleware. `AddProblemDetails()` is configured with a
  `CustomizeProblemDetails` hook that stamps `correlationId` onto **every**
  problem response — both exception-mapped ones and plain status-code
  responses via `UseStatusCodePages()` (e.g. an unmatched route's 404) —
  so the correlation id guarantee in `docs/API.md` holds even when nothing
  threw.
- **DpdpDbContext ships with zero mapped entities in Module 1** —
  intentional. `Modules/Identity`, `Organisations`, `Audit` schemas are
  Module 2/3 work per `docs/MODULE_ROADMAP.md`; this module only proves
  the EF Core → Npgsql → PostgreSQL → migration pipeline works, via a
  migration with an empty model. `dotnet ef` reads its connection string
  from the `ConnectionStrings__Default` environment variable through a
  `IDesignTimeDbContextFactory` (never a literal), so no secret is needed
  in source to run `dotnet ef` commands.
- **The one non-obvious cross-cutting concern added beyond the Module 1
  brief:** baseline security response headers
  (`DPDP.Api/Middleware/SecurityHeadersMiddleware.cs` —
  `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, a
  restrictive `Content-Security-Policy`). Everything else security-related
  in `docs/SECURITY.md` §4 (CORS, rate limiting on auth endpoints, cookie
  handling) is deferred to the module that first has something to protect
  — there are no user-facing endpoints yet to rate-limit.
- **MediatR licensing:** MediatR versions from v13 onward carry a
  commercial licensing tier for organizations above a revenue threshold
  (the same model AutoMapper adopted). It's used here per MASTER_PROMPT's
  explicit "MediatR where useful," but this is a licensing decision, not
  a technical one — see the Module 1 completion report's Known Issues for
  the decision this needs before Phase 2 leans on it more heavily.

## 12. Module 2 (Identity, Authentication & RBAC) — As Implemented

- **`IAppDbContext`, not `DpdpDbContext`, is what Application code depends
  on** (`DPDP.Application/Common/Interfaces/IAppDbContext.cs`) — exposes
  `DbSet<T>` per aggregate plus `SaveChangesAsync`. This is the Clean
  Architecture boundary in practice: every Module 2 command/query handler
  references only this interface, never the concrete Infrastructure type,
  so Application still has zero compile-time dependency on Infrastructure
  even though it uses EF Core's `DbSet<T>`/`IQueryable` directly rather
  than a full repository-per-aggregate abstraction (deliberately — see
  MASTER_PROMPT §23, a repository per aggregate here would be an
  abstraction with no behavior of its own).
- **Tenant isolation, confirmed working as designed**: the `User` global
  query filter (`DpdpDbContext.OnModelCreating`) is the only place tenant
  scoping happens for reads — no command/query handler adds its own
  `WHERE organisation_id = ...` predicate. This was verified two ways:
  `DPDP.IntegrationTests.Identity.TenantIsolationTests` exercises the
  filter directly against a real database with two seeded organisations;
  `DPDP.ApiTests.Identity.UsersTenantIsolationApiTests` exercises the same
  guarantee over real HTTP (404 for another tenant's user by id, filtered
  list, 403 on cross-tenant create). Pre-authentication flows (login,
  refresh, forgot/reset-password) explicitly call `.IgnoreQueryFilters()`
  since there is no tenant context yet at that point in the request — this
  is documented inline at every call site so it doesn't read as an
  accidental bypass.
- **Permission-based authorization is a dynamic policy provider**
  (`PermissionPolicyProvider` + `PermissionAuthorizationHandler` in
  `DPDP.Infrastructure.Security`), turning a policy name like
  `"Permission:users.read"` into a built policy on demand — no
  `AddAuthorization(options => options.AddPolicy(...))` call exists per
  permission. `RequirePermission(string)` (`DPDP.Api/Security/AuthorizationExtensions.cs`)
  is the one-line way an endpoint declares its requirement.
  `PermissionAuthorizationHandler` is registered **Scoped** (it depends on
  the per-request `ICurrentUserContext`).
- **Role-permission template editing is Super-Administrator-only**,
  enforced in the command handler itself, not just via the `roles.manage`
  permission check — see `AssignRolePermissionCommand`'s doc comment.
  Roles are global/shared across every tenant, so this is a deliberately
  narrower gate than the permission system generally implies.
- **User registration is admin-driven, and every user created via
  `POST /api/v1/users` belongs to an organisation** — the one org-less
  Super Administrator account is created exclusively by
  `IdentityBootstrapper` at startup (idempotent, environment-variable
  credentials, skips if any Super Administrator already exists); trying to
  assign the Super Administrator role through `CreateUserCommand` is
  rejected outright. This sidesteps a null-organisation edge case in
  `UserRole`'s uniqueness constraints (see `docs/DATABASE.md` §5) rather
  than handling it.
- **Frontend auth session**: access token in memory only; refresh token in
  `localStorage` — a deliberate trade-off from the original HttpOnly-cookie
  sketch in `docs/SECURITY.md` §4, documented there in full (§12).
  `AuthProvider` (`src/features/auth/AuthProvider.tsx`) is the single
  source of truth; `apiFetch` is wired to it via a small injected-callback
  pattern (`configureApiClient`) rather than importing the store directly,
  to avoid a circular dependency between the API client and the auth
  store's own API calls.

## 13. Module 3 (Organisation Management) — As Implemented

- **Closes the Organisation CRUD gap Module 2 deliberately left open**
  (§12 above): `POST/GET/PUT/DELETE /api/v1/organisations` now exist.
  Create/cross-tenant-list/delete are Super Administrator only (tenant
  provisioning and whole-tenant deletion are platform-level actions); a
  single organisation's own profile is readable/writable by anyone holding
  `organisation.read`/`organisation.write` **for their own tenant only**.
- **A real, non-hypothetical tenant-isolation bug was found and fixed
  here**: `Organisation` *is* the tenant root, so — unlike every other
  entity in the system — it has no `ITenantScoped` global query filter to
  fall back on (there's no parent tenant id to compare against). The first
  pass of `GetOrganisationProfileQuery`/`UpdateOrganisationProfileCommand`/
  `GetOrganisationDashboardQuery`/`CreateOrganisationLocationCommand`
  copied the "the filter already protects this" comment from the User-based
  handlers without noticing it didn't apply — which meant any authenticated
  user of any organisation could read, edit, or add locations to *any*
  other organisation's profile by id. Caught by
  `DPDP.ApiTests.Organisations.OrganisationManagementApiTests` (a
  deliberate cross-tenant test, not a same-tenant happy-path one — see the
  Module 3 completion report). Fixed by an explicit
  `currentUser.IsSuperAdministrator || organisation.Id == currentUser.OrganisationId`
  check in each of those handlers. `OrganisationLocation` itself, by
  contrast, *does* implement `ITenantScoped` (it has a real parent
  organisation), so `UpdateOrganisationLocationCommand`/
  `DeleteOrganisationLocationCommand` were never affected — only lookups
  that start from the Organisation entity itself needed the explicit check.
  **Lesson for future modules**: never copy a "the query filter protects
  this" comment onto a handler without checking whether the entity it
  loads first is actually `ITenantScoped`.
- **`BusinessUnit` and `Department` are the first entities to use the
  generic `ApplyTenantScopedFilter<TEntity>()` helper** added to
  `DpdpDbContext` in this module — the one-line pattern every future
  tenant-scoped entity with a non-nullable `OrganisationId` should reuse,
  rather than hand-writing a query filter lambda per entity.
- **`ContactInfo` is a shared EF Core owned type** (`DPDP.Domain.Common`),
  reused for an organisation's primary/privacy/DPO contacts and a business
  unit's/department's "head" contact — one C# type, mapped to differently
  prefixed columns per owner via `OwnsOne(...).Property(...).HasColumnName(...)`,
  rather than three/four near-duplicate contact tables or flat property sets.
- **Department carries a denormalized `OrganisationId`** copied from its
  parent `BusinessUnit` at creation time (never trusted from client input)
  specifically so it can implement `ITenantScoped` directly and get the
  same one-line filter as `BusinessUnit`, instead of needing a join through
  `BusinessUnit` on every tenant-scoped query. The invariant (a
  Department's `OrganisationId` always equals its `BusinessUnit`'s) is
  enforced only in `CreateDepartmentCommandHandler`, since Department never
  changes parent business unit after creation.
- **Deletes are conservative by design**: an Organisation with any active
  user, or a BusinessUnit with any Department, refuses to delete
  (`ConflictException`, 409) rather than cascading. This is a deliberate
  product choice, not a technical limitation — a later module could add an
  explicit "reassign or force" flow if that's ever needed.
- **A tracking-query bug surfaced by owned types**: `GetBusinessUnitsQuery`'s
  original `.Select(b => new { ..., b.Head, ... })` threw at runtime
  ("owned entities cannot be tracked without their owner") because the
  query wasn't `.AsNoTracking()`. Fixed there and swept across every
  pure-read query handler in the codebase (Module 2's included) as a
  correctness and performance improvement — a read-only query should never
  track by default.

## 14. Module 4 (DPDP Compliance Framework & Control Library) — As Implemented

- **The legal-content graph anticipated in §6 is now real**: `Framework`
  (e.g. the DPDP Act, 2023) → `FrameworkVersion` (e.g. "2023") →
  `LegalReference` (a section, e.g. Section 8) → `Requirement` (an
  obligation derived from that section) →(many-to-many via
  `ControlMapping`)→ `Control` (an assessable control, e.g.
  `DPDP-CTRL-001`) → `AssessmentQuestion` → `EvidenceRequirement`, plus a
  standalone `ControlCategory` used only to group controls for browsing.
  Every one of these except `ControlCategory`/`AssessmentQuestion`/
  `EvidenceRequirement` carries a `ContentReviewStatus`
  (`DRAFT` / `LEGAL_REVIEWED` / `APPROVED`) — see
  `docs/COMPLIANCE_CONTENT_GOVERNANCE.md` for what that flag means and how
  it is meant to move.
- **This whole graph is deliberately *not* `ITenantScoped`.** Unlike every
  entity in Modules 2–3, the compliance framework and control library are
  global reference data — one shared, versioned legal-content library every
  organisation reads from, not tenant-owned data. `DpdpDbContext` therefore
  applies **no** tenant query filter to any Compliance entity; only the two
  leaf entities with a delete action (`AssessmentQuestion`,
  `EvidenceRequirement`) get a soft-delete filter. Because there is no
  filter to lean on, every write handler enforces authorization itself:
  `if (!currentUser.IsSuperAdministrator) throw new ForbiddenException(...)`
  is the first line of every Command handler in this module — the same
  pattern Module 2 established for global role-permission templates. Reads
  are open to any authenticated user holding `controls.read`.
- **`ControlStatus` (`DRAFT`/`ACTIVE`/`RETIRED`) is a second, orthogonal
  status axis to `ContentReviewStatus`.** `ControlStatus` answers "is this
  control currently in operational use," `ContentReviewStatus` answers "has
  a human with legal authority verified the wording." The 13 seeded DPDP
  Act controls deliberately ship as `ACTIVE`/`DRAFT` — usable today,
  pending formal legal review — rather than either extreme
  (`DRAFT`/`DRAFT` would hide genuinely useful content; `ACTIVE`/`APPROVED`
  would overstate how rigorously it's been checked). See
  `docs/COMPLIANCE_CONTENT_GOVERNANCE.md`.
- **The organisation-facing visibility rule lives in the query handlers,
  not a global filter**: `GetControlsQuery`, `GetControlByIdQuery`,
  `GetQuestionsQuery`, and `GetEvidenceRequirementsQuery` all force
  `Status == ControlStatus.ACTIVE` for any caller who is not a Super
  Administrator, regardless of what status filter (if any) was requested —
  a Super Administrator alone can see `DRAFT`/`RETIRED` content, e.g. to
  finish authoring a control before activating it. This was a deliberate
  choice to serve both the admin control-library view and the
  organisation-facing read-only view from one query/endpoint rather than
  building two near-duplicate ones (see `docs/API.md`).
- **Retiring a control never deletes anything.** `RetireControlCommand`
  only flips `Status` to `RETIRED`; the control, its questions, and its
  evidence requirements stay in the database and remain visible to a Super
  Administrator (and to any future assessment that historically referenced
  them) — only organisation-facing listings stop showing them. Activating
  and retiring both reject a no-op transition with `ConflictException`
  (409) rather than silently succeeding.
- **`FrameworkVersion.IsCurrent` is exclusive per Framework, enforced in
  the handler, not the database.** `ActivateFrameworkVersionCommand` sets
  the target version's `IsCurrent = true` and unsets it on every other
  version of the *same* Framework in the same transaction — there is no
  unique-partial-index enforcing this at the schema level, since EF Core's
  migration-based `HasData` seeding does not need it and the invariant is
  simple enough to guarantee in one handler.
- **`AssessmentQuestion.OptionsJson` stores a JSON array in a `jsonb`
  column**, serialized/deserialized through `System.Text.Json` at the
  Application layer boundary (`ComplianceMapper` and the Create/Update
  Question command handlers) rather than modelling a separate
  `QuestionOption` table — the option list is small, has no independent
  identity or relationships, and is only ever read/written as a whole unit
  alongside its question.
- **All seed content lives in one file**,
  `DPDP.Infrastructure/Persistence/Seed/DpdpActSeedData.cs`, referenced by
  nine `IEntityTypeConfiguration<T>.HasData(...)` calls (one per
  Compliance entity) rather than scattered inline literals — see
  `docs/COMPLIANCE_CONTENT_GOVERNANCE.md` for why this file, and only this
  file, is the reviewable source of legal content, and for the sourcing
  methodology (paraphrase, not verbatim statutory text; every
  `LegalReference` carries a `SourceCitation`; the DPDP Rules, 2025
  framework is seeded as an empty version shell rather than guessing final
  rule numbering before notification).
- **A design bug caught before it compiled**: an early draft of
  `UpdateControlCommand` tried to lazy-load the new `ControlCategory` via
  `db.Entry(control).Reference(...).LoadAsync(...)` — but `IAppDbContext`
  intentionally exposes only `DbSet<T>` properties and `SaveChangesAsync`,
  never the concrete `DbContext.Entry()` API (Application must not depend
  on EF Core internals). Fixed by fetching the category directly:
  `await db.ControlCategories.FirstOrDefaultAsync(...)`.
- **Test-data leak into the shared dev database, caught and fixed
  immediately** (the same class of mistake as Module 3 §13's orphaned
  BusinessUnits): `ComplianceApiTests` creates a scratch `Control` and a
  scratch `FrameworkVersion` directly against the real database to test the
  activate/retire and versioning lifecycles. Because this data is global,
  not organisation-scoped, the Identity fixture's tenant cleanup does not
  touch it — the test class now implements `IAsyncLifetime` itself and
  hard-deletes exactly what it created in `DisposeAsync`, including
  re-activating the original `FrameworkVersion` the versioning test
  temporarily demoted, so a partial run never leaves the shared DPDP Act
  framework without a current version.

## 15. Module 5 (DPDP Compliance Assessment Engine) — As Implemented

- **Seven entities, snapshotted from the control library at creation
  time**: `Assessment` (aggregate root) → `AssessmentScope` (which
  business unit/department this run covers, purely descriptive) →
  `AssessmentControl` (one row per Control the assessment covers) →
  `AssessmentControlQuestion` (one row per question under that control) →
  `AssessmentAnswer` (the mutable response — 1:1 with its question) →
  `AssessmentReview` and `AssessmentApproval` (append-only decision logs).
  `CreateAssessmentCommand` copies every `ControlStatus.ACTIVE` control
  mapped to the chosen `FrameworkVersion` into the assessment's own rows,
  the same "snapshot, don't live-join" principle Module 4 established for
  `Control.Version` — later edits to the shared control library never
  retroactively change an in-flight or completed assessment.
- **Naming collision, resolved deliberately**: the Module 5 brief names
  one entity "AssessmentQuestion," but that name already belongs to
  Module 4's question-bank entity (`DPDP.Domain.Modules.Compliance.AssessmentQuestion`
  — the reusable library question, an entirely different concept from "this
  question as included in this specific assessment"). Named
  `AssessmentControlQuestion` here instead to avoid the collision.
- **No automated applicability rule engine.** "Determine Applicable
  Controls" (the brief's workflow step) is implemented as "every ACTIVE
  control mapped to the chosen framework version," full stop — there is no
  rules engine evaluating `AssessmentScope`/organisation attributes against
  `Control.ApplicableConditions` (which remains free text, per Module 4).
  An assessor marks a specific control `NOT_APPLICABLE` by hand during the
  questionnaire instead. Building a structured applicability-rule engine
  now, with no concrete rules to encode yet, would be exactly the kind of
  premature abstraction MASTER_PROMPT section 23 warns against;
  `AssessmentScope` exists today as scoping/reporting metadata, not a
  filter.
- **Tenant isolation, fully denormalized rather than join-based.** Unlike
  Module 4's Compliance library (deliberately global, no tenant filter at
  all), every one of the seven Assessment entities carries its own
  `OrganisationId` and its own `ITenantScoped` query filter — copied down
  from the parent `Assessment` at creation time, the same denormalization
  precedent Module 3 set for `Department.OrganisationId`. This means a
  handler loading e.g. `AssessmentControlQuestion` by id (as
  `SaveAssessmentAnswerCommand` does) gets tenant enforcement for free from
  the query filter alone, with no explicit `currentUser.OrganisationId`
  check needed in the handler — a stronger, less error-prone guarantee than
  relying on a join back to `Assessment` for every write.
- **Two status axes stay separate, on purpose**: `Assessment.Status`
  (`DRAFT → IN_PROGRESS → SUBMITTED → UNDER_REVIEW → APPROVED/REJECTED →
  ARCHIVED`, with `REJECTED → IN_PROGRESS` via Reopen) governs the
  workflow; `AssessmentControl.Status` / `AssessmentAnswer.Status` (both
  reuse Module 4's `Compliance.AnswerStatus` rather than a duplicate
  near-identical enum) govern per-control/per-question assessment
  *content*. `ControlStatusCalculator` (`DPDP.Domain.Modules.Assessments`,
  a pure static function, unit-tested directly) computes the control-level
  rollup from its required questions' answer statuses — see
  `docs/SCORING.md` section 7 for the exact precedence rules and how N/A
  answers are excluded rather than treated as failures.
- **The scoring engine is genuinely pluggable, not just documented as
  such**: `IComplianceScoringStrategy` (Application layer) is resolved via
  DI; `DefaultComplianceScoringStrategy` is the only implementation today,
  but every number it uses (per-status point values, per-risk-level
  weights) comes from `ScoringOptions`, bound from the `Scoring`
  configuration section — never a hard-coded literal in the formula
  itself. Score is always computed live from current answers via
  `GetAssessmentScoreQuery`, never cached on the `Assessment` row, so a
  displayed score can never be stale. Full formula and the "vacuous
  truth" N/A-handling convention are in `docs/SCORING.md`.
- **Evidence is a JSON field, not a new entity.** MASTER_PROMPT section 10
  lists "evidence" as one of several *optional fields on an answer*, and
  the Module 5 brief's named entity list has no separate Evidence entity —
  so `AssessmentAnswer.EvidenceJson` holds a JSON array of lightweight
  `{ requirementId, description, url }` references, mirroring the
  `OptionsJson` pattern Module 4 already established for
  `AssessmentQuestion.Options`. There is still no file upload/storage
  (MASTER_PROMPT's S3-compatible storage abstraction is reserved for the
  later, dedicated Evidence Management module, roadmap item 13) — evidence
  here is a description and/or a URL only.
- **Evidence validation is enforced at write time, not continuously**:
  `SaveAssessmentAnswerCommand` rejects (`409 Conflict`) an attempt to set
  an answer's status to `PASS`/`PARTIAL` when its question has a mandatory
  `EvidenceRequirement` and no evidence is attached. This is a one-time
  gate on the specific state transition, not an invariant re-checked on
  every read — removing evidence later does not retroactively revert the
  answer's status.
- **A real EF Core pitfall, found and fixed during this module**: adding a
  brand-new child entity *only* via a navigation collection
  (`assessment.Reviews.Add(review)`) rather than also calling
  `db.AssessmentReviews.Add(review)` explicitly caused
  `ReviewAssessmentCommand`/`ApproveAssessmentCommand`/`RejectAssessmentCommand`
  to throw `DbUpdateConcurrencyException` ("expected to affect 1 row(s),
  but actually affected 0"). Root cause: `Entity.Id` is a client-generated,
  non-default `Guid` by construction time (`= Guid.NewGuid()`), so when
  `SaveChanges`'s own change-detection graph walk discovers a new entity
  purely through navigation fixup (not through an explicit `Add`), EF
  Core's "is this new?" heuristic — "default key ⇒ Added, non-default key
  ⇒ assume it already exists" — gets it backwards and marks the entity
  `Modified` (an update to a non-existent row) instead of `Added`. Calling
  `db.Assessments.Add(assessment)` on the aggregate *root* in
  `CreateAssessmentCommand` sidesteps this entirely (`Add()` cascades
  `Added` state to the whole reachable graph regardless of key values),
  which is why the bug only surfaced in the three commands that add a
  single new child to an *already-tracked, already-saved* parent. Fixed by
  adding the explicit `db.Set<T>().Add(...)` call alongside the navigation
  add in all three handlers — a pattern worth remembering for any future
  command that appends one child to an existing aggregate rather than
  building the whole graph at once.
- **Missing a Question include, not an Answer include**: a second bug in
  the same debugging session — `SubmitAssessmentCommand`'s null-check
  (`q.Question.IsRequired`) threw `NullReferenceException` because
  `UpdateAssessmentCommandHandler.LoadForDetailAsync`'s Include chain
  loaded `AssessmentControlQuestion.Answer` but never
  `AssessmentControlQuestion.Question` — the stack trace's compiler-
  generated lambda name pointed at the wrong operand in the `&&`
  expression, which cost real debugging time before a throwaway
  integration test isolated the actual null reference. Fixed by adding the
  missing `.ThenInclude(q => q.Question)`.

## 16. Module 6 (Findings, Risk & Remediation) — As Implemented

- **Three sub-modules, one shared workflow**: `Finding` (`DPDP.Domain.Modules.Findings`)
  → `Risk` (`DPDP.Domain.Modules.Risks`) → `RemediationTask`/`RemediationComment`
  (`DPDP.Domain.Modules.Remediation`) — matching the brief's
  "Assessment → Failed/Partial Control → Finding → Risk → Remediation Task
  → Owner → Due Date → Evidence → Verification → Closure" pipeline exactly.
  `CreateFindingFromAssessmentControlCommand` is the "Assessment → Failed/
  Partial Control → Finding" step: it only accepts an `AssessmentControl`
  whose rollup status is `FAIL` or `PARTIAL` (Module 5's
  `Compliance.AnswerStatus`), auto-populates severity from the underlying
  `Control.RiskLevel`, and is idempotent — a second attempt against the
  same control is a 409, not a duplicate Finding.
- **"Asset" has no real FK yet.** The brief lists "Asset" as a Finding
  field, but Asset Inventory (roadmap item 16) is a later, not-yet-built
  module — there is no `Asset` entity to point a foreign key at yet.
  `Finding.AssetReference` is a free-text placeholder column, documented
  in the entity itself, with the intent that it becomes a real FK once
  that module ships. This is a deliberate, temporary compromise, not an
  oversight.
- **"Finding ID"/"Risk ID" are computed display values, not stored
  strings.** `Finding.SequenceNumber`/`Risk.SequenceNumber` are Postgres
  identity columns (atomic under concurrent creation, no race condition);
  `FindingMapper.DisplayNumber`/`RiskMapper.DisplayNumber` format them as
  `FIND-00001`/`RISK-00001` at the Application layer boundary. Storing a
  pre-formatted string would risk it drifting from the real sequence.
- **Two independent status machines, each with its own pure
  transition-rule function** (`FindingStatusTransitions`,
  `RemediationStatusTransitions` — both in Domain, no database access,
  unit-tested directly): `Finding.Status` runs
  `OPEN → ASSIGNED → IN_PROGRESS → PENDING_VERIFICATION → RESOLVED →
  CLOSED`, with `ACCEPTED_RISK` reachable from any non-terminal status as
  an alternate, terminal exit. `RemediationTask.Status` runs
  `OPEN → IN_PROGRESS → PENDING_VERIFICATION → VERIFIED → CLOSED`. In both
  machines, the terminal-adjacent transitions (`CLOSED` on Finding;
  `VERIFIED`/`CLOSED` on RemediationTask) are deliberately reachable only
  through their own dedicated commands (`CloseFindingCommand`,
  `AcceptFindingRiskCommand`, `VerifyRemediationTaskCommand`,
  `CloseRemediationTaskCommand`), never through the generic
  `UpdateFindingStatusCommand`/`UpdateRemediationTaskStatusCommand` —
  because those closing-authority actions are gated by a different,
  stricter permission (`findings.close` vs. `findings.assign`) than
  ordinary progress updates.
- **Verifying a remediation task deliberately does not cascade to close
  its Finding.** `VerifyRemediationTaskCommand` only needs
  `remediation.manage`; closing a Finding needs the stricter
  `findings.close`. Auto-cascading the Finding's status on remediation
  verification would let a `remediation.manage` holder effectively close a
  finding without ever holding `findings.close` — a privilege-escalation
  shaped bug avoided by keeping the two status machines independently
  operated by explicit, separately-permissioned actions. The one
  deliberate cross-entity effect that *does* exist is the other direction
  and is loosening, not closing: creating the *first* remediation task
  under an `OPEN`/`ASSIGNED` finding moves it to `IN_PROGRESS`, since a
  task existing means work has genuinely started.
- **Risk methodology is pluggable, the same shape as Module 5's compliance
  scoring engine**: `IRiskScoringStrategy` resolved via DI,
  `DefaultRiskScoringStrategy` the only implementation, every weight and
  threshold sourced from `RiskScoringOptions` (configuration), never a
  hard-coded literal in the formula. Full formula in
  `docs/RISK_METHODOLOGY.md`.
- **Notifications are abstracted, not built.** `INotificationService`
  (Application layer interface) is the one call site every
  "someone should be told about this" event goes through (finding
  assigned, remediation task assigned). `LoggingNotificationService`
  (Infrastructure) is the only implementation today — it logs the
  notification instead of delivering it. The real Notifications module
  (email/Teams, user preferences, digests) is roadmap item 32, a later,
  dedicated module; swapping the DI registration for a real implementation
  requires no change to any call site.
- **Evidence, again, is a JSON field, not a new entity** —
  `RemediationTask.EvidenceJson` mirrors `AssessmentAnswer.EvidenceJson`
  from Module 5 exactly (a JSON array of `{ description, url }`
  references, no file upload/storage yet).
- **No new permissions were needed** — `findings.read/create/assign/close`,
  `risks.read/manage`, and `remediation.read/manage` were all already
  seeded in Module 2's forward-looking `PermissionKeys` catalogue. One
  real gap was found and fixed in this module: neither Compliance Officer
  nor Security Officer had `findings.create` (only Privacy Officer did),
  which would have made the two roles most likely to generate findings
  from failed assessments unable to actually create them. Fixed by adding
  `findings.create` to both roles' seeded permission sets (a new,
  additive migration — see `docs/DATABASE.md` Module 6 section). Along
  the way, Compliance Officer also picked up `risks.manage` (previously
  only Security Officer had it) — a deliberate decision, not an accident:
  Compliance Officer already manages Controls/Assessments/Remediation
  end-to-end in this system, so owning the Risk Register fits the same
  "compliance owner" shape.
- **This module's command handlers avoided the Module 5 `Add`-via-
  navigation pitfall entirely** (see §15 above) by explicitly calling
  `db.Set<T>().Add(...)` for every new child entity (new `RemediationTask`,
  `RemediationComment`, `Risk` created from a Finding) rather than relying
  solely on collection-navigation fixup — a direct application of that
  lesson, and the reason every Module 6 API test passed on the first run
  with no `DbUpdateConcurrencyException` debugging needed this time.

## 17. Module 7 (Evidence Management) — As Implemented

- **`EvidenceItem` renamed from the brief's plain "Evidence"** to avoid
  ambiguity with "evidence" as an overloaded term already used elsewhere
  in this codebase (Module 4's `Compliance.EvidenceRequirement`, Module
  5/6's `EvidenceJson` fields) — the same collision-avoidance pattern as
  Module 5's `AssessmentControlQuestion`.
- **"Evidence"/"EvidenceFile" from the brief became `EvidenceItem` +
  `EvidenceVersion`**, not a separate file-record type: an evidence item's
  file content is versioned (re-upload replaces the current file, keeping
  history), so "the current file" and "a specific past file" are both
  just rows in the same `EvidenceVersion` table, distinguished by
  `VersionNumber`. `EvidenceReviewRecord` is a third, append-only entity —
  one row per approve/reject decision, tied to the version number that was
  reviewed at the time (a version can be superseded by a later upload
  without losing the history of what was said about the version that
  actually existed when reviewed).
- **Files never touch PostgreSQL.** `EvidenceItem`/`EvidenceVersion` store
  only metadata; file bytes live behind `IObjectStorageService`
  (`FileSystemObjectStorageService` today — local disk, swappable). Full
  rationale, the malware-scanning abstraction, and the file-validation
  pipeline (extension allow-list → content-type match → magic-byte
  signature → checksum → scan → size limit) are in
  `docs/EVIDENCE_STORAGE.md`, the same "interface + swappable
  implementation, both resolved via DI" shape as Module 5's
  `IComplianceScoringStrategy` and Module 6's `IRiskScoringStrategy`/
  `INotificationService`.
- **`EvidenceFileProcessor` is a shared internal helper**, extracted
  after `UploadEvidenceCommand` and `UploadEvidenceVersionCommand` were
  found to need the identical validate→checksum→scan→store pipeline —
  the same "shared `*Loader`" precedent Module 6 set with
  `FindingLoader`/`RemediationTaskLoader`, here applied to a write-path
  pipeline rather than a read-path Include chain.
- **Vendor and Processing Activity have no real FK yet**, the same
  temporary-placeholder pattern as Module 6's `Finding.AssetReference`:
  `EvidenceItem.VendorReference`/`ProcessingActivityReference` are
  free-text columns until Vendor Management (roadmap item 28) and
  Processing Activities (roadmap item 20) exist.
- **Separation of duties is a deliberate design, not a gap.**
  `evidence.upload` and `evidence.review` are held by disjoint role sets
  in the default templates (Privacy Officer/IT Administrator/Department
  Owner upload; Compliance Officer/Security Officer review) — an
  anti-self-approval control. This is the mirror image of the real
  `findings.create` gap the architecture review found and fixed in Module
  6: there, two roles were *missing* a permission they clearly needed;
  here, two role groups *not* overlapping on upload/review is intentional
  and is asserted directly by an API test
  (`Uploader_role_cannot_review_and_reviewer_role_cannot_upload`).
- **No scheduler exists in this codebase yet**, so the brief's
  APPROVED→EXPIRED transition (driven by wall-clock time, not a user
  action) is exposed as an authenticated, on-demand batch command
  (`MarkEvidenceExpiredCommand`) rather than a background job. See
  `docs/EVIDENCE_STORAGE.md` §6.
- **`EvidenceStatusTransitions` follows the same pure, unit-tested static
  state-machine shape** as `FindingStatusTransitions`/
  `RemediationStatusTransitions`: `UPLOADED → UNDER_REVIEW →
  APPROVED/REJECTED`, `APPROVED → EXPIRED`, and `{APPROVED, REJECTED,
  EXPIRED} → ARCHIVED` (terminal). Uploading a new version unconditionally
  resets status to `UPLOADED` from any non-`ARCHIVED` state — the same
  "restart, don't validate a generic transition" precedent as
  `Finding.Reopen`.
- **Every new child entity is added via explicit `db.Set<T>().Add(...)`**
  (`EvidenceVersion`, `EvidenceReviewRecord`) — the Module 5
  `Add`-via-navigation lesson applied proactively from the start, as in
  Module 6; zero `DbUpdateConcurrencyException`s during development.

## 18. Module 8 (Data Discovery Engine) — As Implemented

- **Connector/agent model, not a data pipeline.** Per the brief's core
  security principle, no code path ever copies bulk customer data into
  this platform. `IDiscoveryConnector` implementations
  (`PostgresDiscoveryConnector`/`MySqlDiscoveryConnector`/
  `SqlServerDiscoveryConnector`/`FileSystemDiscoveryConnector`, all
  Infrastructure) read only catalog metadata, engine-native row-count
  *estimates*, and a small `LIMIT`-bounded sample used purely to derive
  one masked example value per column. Full rationale, the read-only
  connection hints, and the sampling/masking contract are in
  `docs/DATA_DISCOVERY.md`.
- **First background-job infrastructure in this codebase.**
  `DiscoveryJobBackgroundService` (`BackgroundService`, Infrastructure) is
  the sole consumer of `IDiscoveryJobQueue` (an in-memory
  `System.Threading.Channels.Channel<Guid>` — explicitly a "first
  version" choice, not durable/distributed — see
  `docs/DATA_DISCOVERY.md` section 5). `StartDiscoveryJobCommand` never
  runs a scan on the request thread; it only ever creates a `PENDING`
  `DiscoveryJob` and enqueues its id.
- **A real bug caught and fixed during development, worth recording as a
  precedent**: `DpdpDbContext`'s tenant query filters read
  `ICurrentUserContext`, which resolves to "no organisation" inside the
  background service's own DI scope (no HTTP request, no authenticated
  user) — every query silently returned nothing, so `DiscoveryJob` rows
  stayed `PENDING` forever with no error logged anywhere. Fixed with
  `.IgnoreQueryFilters()` in `DiscoveryJobProcessor` and the
  background service's cancellation-poll query, the same precedent
  Identity's pre-auth queries (Login/Refresh/ResetPassword, section 11)
  already established for "code that legitimately runs outside a
  tenant-authenticated request." Any future background/scheduled job in
  this codebase should expect the same issue and apply the same fix.
- **`DataAsset` is upserted across runs, `DiscoveryResult` is not.**
  `DataAsset`/`DataElement` are the canonical, current-state rows
  (identity = `DataSourceId + SchemaName + AssetName` for an asset,
  `+ ColumnName` for an element) — a second scan of the same table
  updates the same rows rather than creating duplicates.
  `DiscoveryResult` is a new, append-only row per job per asset,
  preserving the history of what each specific run observed (row count at
  that time, columns discovered, indexes at that time).
- **A human's classification correction is permanent.**
  `DiscoveryJobProcessor` skips re-running `IDataClassificationStrategy`
  for any `DataElement` with `IsHumanCorrected = true` — a later re-scan
  of the same column never silently reverts a human's decision. Verified
  by `DataDiscoveryApiTests.Full_pipeline_...`, which re-runs discovery
  after a human correction and asserts it survives.
- **Classification is rule-based, not ML, per the brief.**
  `DefaultDataClassificationStrategy` matches column names against
  configurable keyword lists (`ClassificationOptions`) — the same
  "interface + options, resolved via DI" shape as Module 5's
  `IComplianceScoringStrategy` and Module 6's `IRiskScoringStrategy`. See
  `docs/DATA_DISCOVERY.md` section 4 for why the masked-sample signal is
  deliberately weak, and why `OTHER_PERSONAL_DATA` is never
  auto-assigned.
- **Connection secrets encrypted via ASP.NET Core Data Protection**
  (`ConnectionSecretProtector`), not a hand-rolled cipher — framework
  -managed keys, persisted to a configurable path (gitignored) so
  `DataSource.EncryptedSecret` survives an API process restart. No DTO
  ever exposes the secret or its ciphertext. See
  `docs/DATA_DISCOVERY.md` section 2.
- **`SchemaFilter` added to `DataSource`, beyond the brief's literal
  entity list**, to let a Postgres/SQL Server scan be bounded to one
  schema instead of an entire database — both a genuinely useful
  first-version feature for a large customer database and the mechanism
  the module's own API test uses to scan a real local Postgres schema
  without ever touching this platform's own tables.
- **No live MySQL/SQL Server instance exists in this environment** — see
  `docs/DATA_DISCOVERY.md` section 8 for what that means for test
  coverage on those two connectors.

## 19. Module 9 (Data Inventory & Processing Activities) — As Implemented

- **Two naming collisions avoided, one entity folded away entirely.**
  "System" → `ItSystem` (avoids colliding with the .NET `System`
  namespace); the brief's "Data Source" → `DataCollectionSource` (avoids
  colliding with Module 8's unrelated `DataSource`, a technical discovery
  -connector entity); the brief's "Data Element" was **not** given a new
  entity at all — Module 8 already owns `DataElement` for a discovered
  column, so `DataInventoryItem.DataElementName` is a plain string field,
  with an optional `DiscoveredDataElementId` bridging to a real Module 8
  `DataElement` when an inventory entry originated from a discovery scan.
  Full rationale in `docs/DATA_INVENTORY.md` section 1.
- **Nine independent tenant-scoped aggregate roots, no cascade
  relationships between them.** Six shared catalogues
  (`DataCategory`/`ItSystem`/`DataCollectionSource`/`Processor`/
  `Recipient`/`RetentionPolicy`) are referenced by both `DataInventoryItem`
  (single nullable FK each) and `ProcessingActivity` (many-to-many for
  five of the six, single nullable FK for `RetentionPolicy`) — the same
  "shared master data, cross-referenced rather than owned" shape as
  Module 8's `DataAsset`/`DataElement` relationship to `DataSource`. See
  `docs/DATA_INVENTORY.md` section 2.
- **A brand-new workflow shape for this codebase**: two distinct
  "back to DRAFT" commands
  (`SendProcessingActivityBackToDraftCommand` from `IN_REVIEW`,
  `ReopenProcessingActivityCommand` from `APPROVED`) both land on the same
  status but are deliberately kept semantically non-overlapping via an
  explicit precondition check in each handler rather than relying solely
  on the general `ProcessingActivityStatusTransitions.CanTransition`
  table (which would allow either from either state) — see
  `docs/DATA_INVENTORY.md` section 3.
- **Review/approval fields are flattened onto `ProcessingActivity`
  itself**, not modeled as separate child entities the way Module 5's
  `AssessmentReview`/`AssessmentApproval` are — a deliberate
  simplification since this brief's workflow is a single review→approve
  chain per submission, not iterative multi-round review.
- **Separation of duties**: `processingactivities.manage` and
  `processingactivities.review`/`.approve` are held by disjoint roles
  (Privacy Officer manages; Compliance Officer reviews/approves) — the
  same anti-self-approval shape Module 7 established between evidence
  upload and review.
- **This platform's first CSV export** — a small, dependency-free
  `CsvWriter` (`DPDP.Application.Common.Csv`) backs both
  `GET /api/v1/data-inventory/export` and
  `GET /api/v1/processing-activities/export`; reusable by any future
  module needing one rather than each pulling in its own CSV library.
- **`DataFlow` is metadata only** — it records that a movement of data is
  known to exist (for compliance mapping, especially cross-border
  transfer visibility), never an actual data pipe or integration. See
  `docs/DATA_INVENTORY.md` section 5.

## 20. Module 10 (Consent & Privacy Operations) — As Implemented

- **`DataPrincipal` never stores a name, email, or phone number** — only
  an `ExternalReferenceId` pointer into the organisation's own system of
  record, plus an optional `ReferenceCategory` (reusing Module 9's
  `DataSubjectCategory` rather than duplicating it). This is the
  architectural answer to the brief's "never store unnecessary raw
  personal data." `DataPrincipalRequest` is the deliberate exception —
  it carries the identity claimed at intake, since corresponding about
  one specific request requires knowing who to write back to. See
  `docs/CONSENT_PRIVACY_OPERATIONS.md` section 1.
- **`PrivacyNotice` versioning is `(Code, Version)`**, strictly linear
  DRAFT → APPROVED → PUBLISHED → ARCHIVED with no reopening — a
  published notice's content is never edited; a correction is a new
  `PrivacyNotice` row with the same `Code`, a new `Version`, and its own
  approval cycle. `ConsentRecord.NoticeVersionId` always points at one
  exact version. See `docs/CONSENT_PRIVACY_OPERATIONS.md` section 2.
- **`ConsentRecord` is deliberately not `ISoftDeletable` and has no
  delete endpoint** — modeled as compliance evidence, the same
  append-only shape as `AuditLog`, using `ApplyTenantOnlyFilter` instead
  of `ApplyTenantScopedFilter`. `WithdrawConsentCommand`
  (data-principal-initiated) and `RevokeConsentCommand`
  (organisation-initiated, requires a `Reason`) are kept as two distinct
  commands despite sharing a target status, because who invalidated the
  consent and why matters for the audit trail. See
  `docs/CONSENT_PRIVACY_OPERATIONS.md` section 3.
- **"Grievances" folded into `DataPrincipalRequest.RequestType =
  GRIEVANCE`**, not a separate entity — the brief gives Data Principal
  Requests and Grievances one identical field shape and workflow, the
  fourth time this codebase has reconciled originally-separate roadmap
  items into one entity (after Modules 6, 8, and 9). See
  `docs/CONSENT_PRIVACY_OPERATIONS.md` section 4.
- **SLA due dates come from a fully configurable `SlaPolicy` catalog,
  never a hard-coded day-count** — per the brief's explicit "do not
  hard-code legal deadlines without verified legal source/
  configuration." No default policy is seeded; `DueAt` is `null` unless
  a matching policy (type-specific, or a `RequestType == null`
  catch-all) is configured. Only one active policy per `RequestType` is
  allowed. See `docs/CONSENT_PRIVACY_OPERATIONS.md` section 5.
- **Portal-ready by construction, not by building a portal** —
  every data-principal-facing field is plain data, never a foreign key
  into the internal `User`/RBAC system, so a future public self-service
  portal could plug into the same domain model without redesign. No
  unauthenticated endpoint exists in this module. See
  `docs/CONSENT_PRIVACY_OPERATIONS.md` section 6.
- **Separation of duties**: `privacynotices.manage` (Privacy Officer)
  and `privacynotices.approve` (Compliance Officer) are held by disjoint
  roles, the same anti-self-approval shape as Module 9's Processing
  Activity manage/approve split.
