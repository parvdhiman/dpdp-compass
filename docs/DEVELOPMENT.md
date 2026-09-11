# DPDP-COMPASS — Development Guide

## 1. Prerequisites

| Tool | Version used in this repo |
|---|---|
| .NET SDK | 10.0.111 |
| Node.js | v22.22.0 |
| npm | 10.9.4 |
| PostgreSQL | 18.x, reachable and already provisioned (see below) |
| Docker + Docker Compose | for Redis/MinIO and optional containerised runs |

This project assumes PostgreSQL is already running and reachable — it does
not manage the PostgreSQL *server* for you. `deployment/docker-compose.yml`
has an opt-in `local-db` profile if you don't already have one.

## 2. First-Time Setup

```bash
# 1. Backend: restore + install the EF Core CLI tool
cd src/Backend
dotnet tool install --global dotnet-ef --version 10.*
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet restore DPDP.slnx

# 2. Give the Api project your local connection string via user-secrets —
#    never put a real password in appsettings.*.json or .env.
dotnet user-secrets set "ConnectionStrings:Default" \
  "Host=localhost;Port=5432;Database=dpdp_compass;Username=<user>;Password=<password>" \
  --project DPDP.Api

# 3. Apply migrations (dotnet ef reads ConnectionStrings__Default from the
#    environment for CLI use — see docs/ARCHITECTURE.md section 11):
export ConnectionStrings__Default="Host=localhost;Port=5432;Database=dpdp_compass;Username=<user>;Password=<password>"
dotnet ef database update --project DPDP.Infrastructure --startup-project DPDP.Api

# 4. Give the Api project a JWT signing key via user-secrets — never
#    commit one. Any sufficiently random string works for local dev.
dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 48)" --project DPDP.Api

# 5. Frontend
cd ../../src/Frontend/dpdp-web
npm install
cp .env.example .env   # defaults already point at http://localhost:5136
```

### First login — bootstrapping the Super Administrator

There is no signup page (admin-driven registration only, per
`docs/ARCHITECTURE.md`). The very first account and organisation are
created automatically on API startup by `IdentityBootstrapper`, but only
when these environment variables are set **and** no Super Administrator
already exists in the database:

```bash
export BOOTSTRAP_SUPERADMIN_EMAIL="you@example.local"
export BOOTSTRAP_SUPERADMIN_PASSWORD="<something satisfying the password policy — see docs/SECURITY.md>"
export BOOTSTRAP_ORGANISATION_NAME="My Organisation"   # optional, defaults to "Default Organisation"
```

Run `dotnet run` (from `DPDP.Api`) once with these set; after that first
successful run you can unset them — the bootstrapper is a no-op once a
Super Administrator exists. From there, sign in at the frontend's `/login`
and use `POST /api/v1/users` (as an Organisation Administrator you create)
to onboard everyone else.

## 3. Running Locally

```bash
# Supporting infra (Redis, MinIO) — from the repo root:
docker compose --env-file .env -f deployment/docker-compose.yml up -d
# (copy .env.example to .env at the repo root first)

# Backend (from src/Backend/DPDP.Api):
dotnet run
# -> http://localhost:5136  (see Properties/launchSettings.json)
# Swagger UI at /swagger, health at /health and /health/ready

# Frontend (from src/Frontend/dpdp-web):
npm run dev
# -> http://localhost:5173
```

The frontend's default `VITE_API_BASE_URL` and the backend's default CORS
allowed origin are pre-wired to match these two ports.

## 4. Running Tests

```bash
# Backend — all three test projects (Unit, Integration, Api):
cd src/Backend
export ConnectionStrings__Default="Host=localhost;Port=5432;Database=dpdp_compass;Username=<user>;Password=<password>"
export Jwt__SigningKey="$(openssl rand -base64 48)"
dotnet test DPDP.slnx

# Frontend:
cd src/Frontend/dpdp-web
npm run test        # vitest, run once
npm run test:watch  # vitest, watch mode
npm run lint         # oxlint
```

`DPDP.IntegrationTests` and `DPDP.ApiTests` both require
`ConnectionStrings__Default` to point at a reachable, migrated database —
they are not mocked, by design (see `docs/PROJECT_PLAN.md` guiding
principle: prove tenant isolation and infrastructure wiring against the
real thing, not a stand-in). `DPDP.ApiTests` additionally requires
`Jwt__SigningKey` since it boots the real Api host. Neither project depends
on the `BOOTSTRAP_SUPERADMIN_*` variables — they create their own,
uniquely-named test organisations/users directly and clean them up
afterward, so they work against a shared dev database without disturbing
whatever you've bootstrapped there. CI provides both env vars via
`.github/workflows/ci.yml`, with a fresh PostgreSQL service container.

## 5. Database Migrations

Every module that changes the schema adds its own migration — never edit a
previously-applied migration in place.

```bash
cd src/Backend
export ConnectionStrings__Default="..."
dotnet ef migrations add <Module>_<Description> \
  --project DPDP.Infrastructure --startup-project DPDP.Api \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project DPDP.Infrastructure --startup-project DPDP.Api
```

## 6. Secrets — Never Commit Them

- Backend local dev secrets: `dotnet user-secrets` (stored outside the
  repo, under your home directory) — not a `.env` file, not
  `appsettings.Development.json`.
- Frontend local dev config: `src/Frontend/dpdp-web/.env` (git-ignored).
- Docker Compose: root `.env` (git-ignored), from `.env.example`.
- CI: a dedicated, CI-only database password defined inline in
  `.github/workflows/ci.yml` — it only ever talks to the ephemeral service
  container for that run, so it isn't a real secret.

See `docs/SECURITY.md` section 7 for the full policy.

## 7. Solution Layout Reference

See `docs/ARCHITECTURE.md` section 2 for the full folder structure and
section 11 for what Module 1 concretely built. In short:

- `src/Backend/DPDP.{Domain,Application,Infrastructure,Api}` — the four
  Clean Architecture layers, `DPDP.slnx` ties them together.
- `src/Frontend/dpdp-web` — Vite + React + TypeScript + MUI.
- `tests/DPDP.{UnitTests,IntegrationTests,ApiTests}` — one project per
  test category, referencing the layers they exercise.
- `deployment/` — `docker-compose.yml` and the Dockerfiles it builds
  (which live next to their projects: `src/Backend/DPDP.Api/Dockerfile`,
  `src/Frontend/dpdp-web/Dockerfile`).
