# DPDP-COMPASS

**DPDP Compliance Management & Continuous Assessment Platform.**

Helps organisations assess, manage, monitor, document, and improve their
compliance posture under India's Digital Personal Data Protection Act,
2023. It reports Compliance Assessment Score, Readiness, Control
Effectiveness, Risk Level, Evidence Coverage, Open Findings, Remediation
Status, and Assessment Confidence — it does not, and will not, assert
absolute legal compliance. Legal interpretation stays with the
organisation's own legal/privacy/compliance personnel.

Full governing specification: [`MASTER_PROMPT.md`](./MASTER_PROMPT.md).

## Status

**Phase 5, Module 10 — Consent & Privacy Operations.** Project
foundation, database, identity, authentication, permission-based RBAC,
full organisation management, the versioned DPDP Act 2023 compliance
framework and control library, the assessment engine (create, answer,
submit, review, approve, score — `docs/SCORING.md`), the full
post-assessment workflow (Finding → Risk → Remediation, with a
configurable risk methodology — `docs/RISK_METHODOLOGY.md`), Evidence
Management (upload/version/review with a separation-of-duties control,
behind an object-storage abstraction — `docs/EVIDENCE_STORAGE.md`), the
Data Discovery Engine (a connector/agent model for PostgreSQL/MySQL/
SQL Server/File System that reads only schema metadata and masked
samples, feeding a rule-based classifier — `docs/DATA_DISCOVERY.md`),
and this platform's Record of Processing Activities (a Data Inventory of
what personal data exists and where, a Processing Activity Register with
a Draft→Review→Approve→Archive workflow, and Data Flow metadata —
`docs/DATA_INVENTORY.md`) are all in place and tested. Module 10 adds
Consent & Privacy Operations: versioned Privacy Notices
(Draft→Approve→Publish→Archive), a Consent Purpose catalogue, an
append-only Consent Record ledger (grant/withdraw/organisation-revoke/
expire) tied to a minimal, non-PII `DataPrincipal` reference, and a
unified Data Principal Request workflow covering Access, Correction,
Erasure, Withdraw-Consent, Grievance, and Other requests with
configurable (never hard-coded) SLA due-date tracking
(`docs/CONSENT_PRIVACY_OPERATIONS.md`). See
[`docs/COMPLIANCE_CONTENT_GOVERNANCE.md`](./docs/COMPLIANCE_CONTENT_GOVERNANCE.md)
for how the underlying legal content is sourced and reviewed. See
[`docs/MODULE_ROADMAP.md`](./docs/MODULE_ROADMAP.md) for what's built vs.
planned.

## Architecture

Modular monolith, Clean Architecture layering, multi-tenant from day one.
Full design in [`docs/ARCHITECTURE.md`](./docs/ARCHITECTURE.md).

```
src/
  Backend/
    DPDP.Api             ASP.NET Core 10 Web API (Minimal API endpoints)
    DPDP.Application      CQRS handlers (MediatR), validation, DTOs
    DPDP.Domain           Entities, domain rules — no framework dependencies
    DPDP.Infrastructure    EF Core + PostgreSQL, external service adapters
  Frontend/
    dpdp-web              React + TypeScript + Vite + MUI

tests/
  DPDP.UnitTests
  DPDP.IntegrationTests
  DPDP.ApiTests

docs/          Architecture, database, API, security, module documentation
deployment/    Docker Compose, Dockerfiles are alongside their projects
scripts/       Operational scripts
```

## Tech Stack

ASP.NET Core 10 · C# · Entity Framework Core · PostgreSQL · Redis ·
Serilog · FluentValidation · MediatR · React · TypeScript · Vite · MUI ·
TanStack Query · React Hook Form · Zod. Full rationale in
`docs/ARCHITECTURE.md` section 10.

## Getting Started

See [`docs/DEVELOPMENT.md`](./docs/DEVELOPMENT.md) for full setup.
Quick version:

```bash
# Supporting infra (Redis, MinIO)
cp .env.example .env
docker compose --env-file .env -f deployment/docker-compose.yml up -d

# Backend — requires PostgreSQL reachable; see docs/DEVELOPMENT.md
cd src/Backend
dotnet user-secrets set "ConnectionStrings:Default" "<your connection string>" --project DPDP.Api
export ConnectionStrings__Default="<your connection string>"
dotnet ef database update --project DPDP.Infrastructure --startup-project DPDP.Api
dotnet run --project DPDP.Api
# -> http://localhost:5136 (Swagger at /swagger, health at /health)

# Frontend
cd ../../src/Frontend/dpdp-web
npm install
npm run dev
# -> http://localhost:5173
```

## Testing

```bash
cd src/Backend && dotnet test DPDP.slnx
cd src/Frontend/dpdp-web && npm run test && npm run lint
```

CI (`.github/workflows/ci.yml`) runs both on every push/PR to `main`,
against a real PostgreSQL service container.

## Documentation

| Doc | Covers |
|---|---|
| [`docs/PROJECT_PLAN.md`](./docs/PROJECT_PLAN.md) | Phases, guiding principles, approval gates |
| [`docs/ARCHITECTURE.md`](./docs/ARCHITECTURE.md) | Layering, multi-tenancy, RBAC, compliance engine design |
| [`docs/MODULE_ROADMAP.md`](./docs/MODULE_ROADMAP.md) | All 35 modules, phase, dependencies, status |
| [`docs/DATABASE.md`](./docs/DATABASE.md) | Schema conventions, Phase 1 tables |
| [`docs/API.md`](./docs/API.md) | REST conventions, endpoint surface |
| [`docs/SECURITY.md`](./docs/SECURITY.md) | Threat model, controls, audit logging |
| [`docs/COMPLIANCE_CONTENT_GOVERNANCE.md`](./docs/COMPLIANCE_CONTENT_GOVERNANCE.md) | Sourcing, review workflow, and versioning of legal/compliance content |
| [`docs/SCORING.md`](./docs/SCORING.md) | The configurable compliance scoring engine — formula, N/A handling, configuration |
| [`docs/RISK_METHODOLOGY.md`](./docs/RISK_METHODOLOGY.md) | The configurable risk-scoring methodology — Likelihood/Impact/Data Sensitivity/Exposure |
| [`docs/EVIDENCE_STORAGE.md`](./docs/EVIDENCE_STORAGE.md) | Object-storage abstraction, malware-scanning abstraction, file-validation pipeline |
| [`docs/DATA_DISCOVERY.md`](./docs/DATA_DISCOVERY.md) | The connector/agent model, sampling & masking contract, classification engine, background-job architecture |
| [`docs/DATA_INVENTORY.md`](./docs/DATA_INVENTORY.md) | Data Inventory, Processing Activity Register workflow, Data Flow metadata, naming-collision decisions |
| [`docs/CONSENT_PRIVACY_OPERATIONS.md`](./docs/CONSENT_PRIVACY_OPERATIONS.md) | Privacy Notice versioning, Consent Record lifecycle, Data Principal Request/Grievance workflow, SLA configurability, portal-ready design |
| [`docs/DEVELOPMENT.md`](./docs/DEVELOPMENT.md) | Local setup, running tests, migrations |
| [`docs/DEPLOYMENT.md`](./docs/DEPLOYMENT.md) | Production build, environments, health checks |

## Security

This is a security-sensitive enterprise application from the ground up —
see `docs/SECURITY.md`. Do not commit secrets; `.env.example` documents
required variable names only. Report security concerns to
itsupport@drishinfo.com rather than filing a public issue.

## License / Ownership

Internal project — Drishinfo. Not for external distribution unless
explicitly authorised.
