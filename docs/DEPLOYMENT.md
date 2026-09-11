# DPDP-COMPASS — Deployment

Module 1 (Project Foundation) establishes the deployment mechanics only —
there is nothing DPDP-specific to deploy yet. This document covers what
exists today and the target production shape from `docs/ARCHITECTURE.md`
section 9; it will grow as later modules add real dependencies (object
storage, background jobs, search).

## 1. Environments

Four named environments, controlled by `ASPNETCORE_ENVIRONMENT`:
Development, Testing, Staging, Production. Each has its own
`appsettings.<Environment>.json` for non-secret overrides (log verbosity,
etc.) — secrets are never in any `appsettings.*.json` file, in any
environment; see `docs/SECURITY.md` section 7.

## 2. Building for Production

```bash
# Backend
cd src/Backend
dotnet publish DPDP.Api/DPDP.Api.csproj -c Release -o ./publish

# Frontend
cd src/Frontend/dpdp-web
npm ci
npm run build          # outputs to dist/
```

Or, containerised (see `src/Backend/DPDP.Api/Dockerfile` and
`src/Frontend/dpdp-web/Dockerfile`):

```bash
docker compose --env-file .env -f deployment/docker-compose.yml --profile full build
```

## 3. Configuration in Production

Set via environment variables (systemd `EnvironmentFile=`, Docker secrets,
or your platform's equivalent) — never a committed file:

| Variable | Purpose |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__Default` | PostgreSQL connection string |
| `Cors__AllowedOrigins__0`, `__1`, ... | Exact browser origins allowed to call the API |

See `.env.example` at the repo root for the full documented list as it
exists today.

## 4. Running on Ubuntu Linux (primary target)

Two supported shapes, per `docs/ARCHITECTURE.md` section 9:

**a) Docker Compose** — build the images above, run behind an nginx
reverse proxy that terminates HTTPS and forwards to the `api` and `web`
containers. `deployment/docker-compose.yml`'s `full` profile is the
starting point; add a `certbot`/TLS-terminating reverse proxy service (or
use an existing host-level nginx/Caddy) in front of it for production —
not included yet, since Module 1 has no public-facing content that needs
it.

**b) systemd**, without Docker for the app itself: publish the API with
`dotnet publish`, run it as a systemd service (`ExecStart=dotnet
/opt/dpdp-compass/api/DPDP.Api.dll`, `EnvironmentFile=/etc/dpdp-compass/api.env`),
and serve the frontend's static `dist/` output through nginx, which also
reverse-proxies `/api/` and `/health*` to the API's local port. A concrete
unit file and nginx site config will be added once there's a real
production target to point at — Module 1 doesn't assume host layout
details (users, paths) that later infra work should decide deliberately.

## 5. Database Migrations in Production

Never run `dotnet ef database update` against production ad hoc. Apply
migrations as an explicit, reviewed deployment step:

```bash
dotnet ef database update --project DPDP.Infrastructure --startup-project DPDP.Api
```

using a `ConnectionStrings__Default` scoped to a migration-capable
database role, run from a controlled deployment context (CI job or
operator shell) — never embedded in application startup code, so a bad
migration can't take down a running fleet on its own restart.

## 6. Health Checks

- `GET /health` — liveness only, no dependency checks. Point your
  orchestrator's liveness probe / load balancer health check here.
- `GET /health/ready` — readiness, currently checks PostgreSQL
  connectivity. Point readiness probes here; a Redis check is added once
  a module actually depends on Redis (see `docs/ARCHITECTURE.md`
  section 11).

## 7. What's Deliberately Not Here Yet

- TLS/certificate provisioning (no public endpoint to protect yet beyond
  what a developer's own reverse proxy would handle).
- Kubernetes manifests — explicitly deferred per MASTER_PROMPT §23 until
  there's an operational reason for them.
- Backup/restore runbooks for PostgreSQL — will be documented once the
  schema holds anything worth restoring (Module 2 onward).
