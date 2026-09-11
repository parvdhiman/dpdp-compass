# DPDP-COMPASS — Data Discovery Engine

This document explains Module 8 (Data Discovery Engine): the
connector/agent model, why the platform never copies customer data at
scale, the sampling and masking contract, the classification engine, and
the background-job architecture. It follows the same "interface +
swappable implementation(s), resolved via DI" shape as prior modules'
scoring/storage abstractions (`docs/SCORING.md`, `docs/RISK_METHODOLOGY.md`,
`docs/EVIDENCE_STORAGE.md`).

## 1. The connector/agent model

Per the brief's core security principle — **the platform must not copy
complete customer databases into the central platform** — every connector
(`IDiscoveryConnector`, `DPDP.Application.Modules.DataDiscovery.Connectors`)
reads only:

- **Catalog/schema metadata** — table/column/type/nullability/index
  definitions, via each engine's own catalog views
  (`information_schema`/`pg_catalog` for Postgres,
  `information_schema` for MySQL, `sys.*` for SQL Server) — never a data
  export.
- **Approximate row counts** from the engine's own statistics (Postgres
  `pg_class.reltuples`, MySQL `information_schema.tables.table_rows`, SQL
  Server `sys.partitions.rows`) — never a live `COUNT(*)`, which would be
  an expensive full scan on a large customer table.
- **A small, bounded sample** (`DiscoveryOptions.SampleRowLimit`, default
  5 rows) per table, fetched with `LIMIT`/`TOP` at the SQL level — never
  an unbounded read — purely to derive one masked example value per
  column. The fetched rows exist only for the duration of one method call
  and are never logged, persisted, or returned past the connector
  boundary in raw form.

Four connectors exist today, one per `DataSourceType`:
`PostgresDiscoveryConnector`, `MySqlDiscoveryConnector`,
`SqlServerDiscoveryConnector`, `FileSystemDiscoveryConnector` — all in
`DPDP.Infrastructure.DataDiscovery.Connectors`, resolved via
`IDiscoveryConnectorResolver` (`IEnumerable<IDiscoveryConnector>` matched
by `SupportedType`). Adding a fifth engine is a new class implementing the
interface plus one DI registration line — no other code changes.

### Read-only defense in depth

Each database connector opens its connection with the strongest
available read-only hint for that engine, on top of only ever issuing
`SELECT`/catalog-view statements:

- **Postgres**: `Options=-c default_transaction_read_only=on` — a hard
  server-enforced block on any write, not just a client-side convention.
- **MySQL**: `SET SESSION TRANSACTION READ ONLY` issued right after
  connecting (best-effort — some hosted MySQL configurations deny the
  discovery account permission to run it; a denial doesn't block
  discovery, it just means this particular defense-in-depth layer isn't
  active for that data source).
- **SQL Server**: `ApplicationIntent=ReadOnly` on the connection string —
  a hint most meaningful against an AlwaysOn readable secondary; on a
  primary it doesn't hard-block writes the way Postgres's setting does,
  but it is still the idiomatic way to declare read-only intent to
  SqlClient.

None of this is a substitute for least-privilege database credentials —
operators should still create a dedicated, read-only discovery account on
the customer system. These settings are an additional safety net in case
that account is ever over-provisioned or a future code change introduces
a write by mistake.

### Optional schema scoping

`DataSource.SchemaFilter` (Postgres/SQL Server only — MySQL's
"information_schema per connected database" model already scopes a scan
to one database) restricts a scan to a single named schema instead of
every non-system schema in the database. This bounds the blast radius —
and the time — of a scan against a large, multi-schema customer database,
and lets an operator target a specific schema deliberately. Leaving it
unset scans every non-system schema.

## 2. Connection secrets

`DataSource.EncryptedSecret` stores a password/connection key encrypted
with **ASP.NET Core Data Protection**
(`DPDP.Infrastructure.Security.ConnectionSecretProtector`,
`IDataProtectionProvider.CreateProtector("DPDP.DataDiscovery.ConnectionSecrets.v1")`)
— framework-managed key handling rather than a hand-rolled cipher. Keys
are persisted to `DataProtection:KeysPath` (default `dataprotection-keys`,
resolved relative to the API host's working directory, gitignored) —
**without this persistence, every DataSource's secret becomes
undecryptable after every API process restart**. A production deployment
running more than one API instance, or wanting keys to survive a
redeploy to a fresh container, must point this at a shared, durable
location.

`DataSourceConnectionInfoFactory` is the single call site that decrypts a
secret (`DPDP.Application.Modules.DataDiscovery.Connectors`) — both
`TestDataSourceConnectionCommand` and `DiscoveryJobProcessor` go through
it rather than calling `IConnectionSecretProtector.Unprotect` directly, so
there is one reviewable place where plaintext exists in memory. No DTO,
list, or detail response ever includes the secret or its ciphertext —
`DataSourceSummaryDto`/`DataSourceDetailDto` simply have no such field.

## 3. Sampling and masking contract

`DPDP.Domain.Modules.DataDiscovery.SampleMasker` is the **only** function
in this codebase allowed to see a raw sample value from a customer data
source. Every connector calls it before a value ever leaves connector
code — `DiscoveredColumn` (the Application-layer model connectors return)
has no raw-value property at all, by construction, so a connector cannot
pass an unmasked value up the stack even by accident.

Masking rule (matches the brief's example exactly):
`9876543210` → `98******10` — the first two and last two characters stay
visible, everything between becomes `*`; a value of four characters or
fewer is masked completely (showing 2+2 of a 4-character value would
disclose all of it). This is a generic, format-agnostic rule — it treats
a phone number, an email address, or a name the same way. Every unit test
in `SampleMaskerTests` documents its exact edge-case behavior.

`DataElement.SampleMaskedValue` is the only sample-related column in the
schema — there is no raw-value column anywhere in Module 8's tables.

## 4. Classification

`IDataClassificationStrategy` / `DefaultDataClassificationStrategy`
(`DPDP.Application.Modules.DataDiscovery.Classification`) — the same
"interface + options, resolved via DI" shape as
`IComplianceScoringStrategy` (Module 5) and `IRiskScoringStrategy`
(Module 6). **Rule-based only — no ML, no external service, no LLM call**,
per the brief's "do not implement advanced AI."

The classifier reasons over two signals, in this priority order:

1. **Column name** (the dominant, reliable signal) — matched against
   per-category keyword lists in `ClassificationOptions.ColumnNameKeywords`
   (fully configurable, no keyword hard-coded in the strategy class
   itself). An exact token match (e.g. a column literally named `email`)
   scores `ExactMatchConfidence` (95 by default); a substring match (e.g.
   `customeremail`) scores the lower `ContainsMatchConfidence` (75).
2. **The masked sample's shape** — deliberately weak, since masking
   destroys most of a value's signal by design. The only thing checked is
   whether the sample's *visible* (non-asterisk) characters are all
   digit/punctuation-shaped, which nudges confidence up for
   IDENTIFIER/CONTACT/FINANCIAL/LOCATION matches (`SamplePatternBonus`,
   capped so total confidence never exceeds 99). A masked email's
   visible prefix/suffix rarely looks digit-shaped, so this signal mostly
   helps numeric identifiers/phone numbers, not text fields — an
   intentional, honestly-scoped limitation given the classifier is never
   allowed to see the raw value.

`OTHER_PERSONAL_DATA` has **no keyword list** and is therefore never
auto-assigned — it exists solely for a human reviewer to apply to a
column they can see is personal data but that doesn't fit a more specific
category. Guessing at this catch-all automatically would be a confident
-sounding but low-information classification; better to leave it to a
person.

### Human correction — and why it survives re-scans

`UpdateDataElementClassificationCommand` lets a `classification.review`
holder set (or clear, via `category: null`) a `DataElement`'s
classification. A human correction always fully replaces the system's
suggestion at 100% confidence, `ClassificationSource.HUMAN`, and sets
`IsHumanCorrected = true`. `DiscoveryJobProcessor` checks this flag before
ever re-running the system classifier during a later scan of the same
column — **a human's classification is permanent until a human changes
it again**, never silently overwritten by a subsequent discovery run. See
`DataDiscoveryApiTests.Full_pipeline_...` for the test asserting this
survives a second scan.

### Not a legal determination

Per the brief's explicit instruction, nothing in this module asserts a
DPDP Act compliance conclusion — a classification is a categorization
suggestion for a human to act on, never phrased or treated as "this
column violates the Act" or similar. UI copy (see the frontend section)
should keep this framing.

## 5. Background job architecture

`StartDiscoveryJobCommand` only ever creates a `PENDING` `DiscoveryJob`
row and hands its id to `IDiscoveryJobQueue` — the scan itself always
runs off the request thread, inside `DiscoveryJobBackgroundService`
(`DPDP.Infrastructure.DataDiscovery`, a `BackgroundService` registered via
`AddHostedService`).

```
PENDING → RUNNING → COMPLETED
                   → FAILED
                   → CANCELLED
PENDING → CANCELLED   (never dequeued)
```

Enforced by `DiscoveryJobStatusTransitions` (pure, unit-tested, same
shape as `FindingStatusTransitions`/`EvidenceStatusTransitions`).

### Deliberate "first version" scope

- **`IDiscoveryJobQueue`** (`DiscoveryJobQueue`, Infrastructure) is a
  single-process, in-memory `System.Threading.Channels.Channel<Guid>` —
  not a durable/distributed queue. A `PENDING` job left behind by an API
  process restart is simply never picked up. Swapping to a durable queue
  (e.g. backed by the database itself, or a real message broker) later is
  a new implementation of this one interface.
- **One worker, sequential processing** —
  `DiscoveryJobBackgroundService.ExecuteAsync` processes jobs one at a
  time from the channel. A future version could run N workers reading the
  same channel for parallelism; not needed for a first version.
- **Cancellation is bridged from two sources into one `CancellationToken`**:
  `IDiscoveryCancellationRegistry` (in-memory, immediate — a
  `CancelDiscoveryJobCommand` call signals the exact in-flight job's
  token straight away) and `DiscoveryJob.CancellationRequested` (a
  persisted flag, polled from a fresh DB scope every 2 seconds — the
  fallback that guarantees eventual cancellation even if the registry
  can't reach the job, e.g. after a process restart). Either path
  cancels the same linked token that `IDiscoveryConnector.DiscoverAsync`
  checks between assets.
- **No scheduler / recurring scans** — every job is started on demand via
  `POST /api/v1/data-sources/{id}/discovery-jobs`. Recurring discovery
  (e.g. nightly) is a natural future enhancement (an `IHostedService` that
  periodically calls the same command), not built now.

### Tenant filtering is deliberately bypassed inside the background service

`DiscoveryJobProcessor` and `DiscoveryJobBackgroundService`'s
cancellation-poll query both call `.IgnoreQueryFilters()`. This is
required, not incidental: `DiscoveryJobBackgroundService` creates its own
DI scope outside of any HTTP request, so `ICurrentUserContext` (which
`DpdpDbContext`'s tenant query filters read) resolves to "no
organisation" there — without `IgnoreQueryFilters()`, the filter would
silently exclude every row and the processor would see every job as
nonexistent (this was caught during development: jobs stayed `PENDING`
forever with no error, because `FirstOrDefaultAsync` simply returned
`null`). This is the same precedent Identity's pre-authentication queries
already established for Login/Refresh/ResetPassword — see
`docs/ARCHITECTURE.md` section 11. It is safe here because the background
service only ever touches the exact `jobId` it was handed, which was
already validated as belonging to a real organisation by
`StartDiscoveryJobCommand` running under a real authenticated request; it
never runs an open-ended, unscoped query.

### Concurrency limit

`DiscoveryOptions.MaxConcurrentJobsPerOrganisation` (default 3) caps how
many `PENDING`/`RUNNING` jobs one organisation may have at once —
`StartDiscoveryJobCommand` returns 409 Conflict past the limit. This
logic is a simple count-and-compare and is not independently covered by
an HTTP test — reliably hitting the limit through a live HTTP test would
require an artificially slow discovery target to keep jobs occupying
`RUNNING` state long enough to race a second request, which isn't
available in this environment; the code path is simple enough to verify
by inspection.

## 6. Entities

`DataSource` → `DiscoveryJob` → `DiscoveryResult` (a per-run, per-asset
snapshot — row count/column count/indexes *as observed by that specific
job*) alongside `DataAsset` (the canonical, upserted-across-runs "current
state" of one discovered table/view/file, identity =
`DataSourceId + SchemaName + AssetName`) → `DataElement` (one discovered
column, or — for a non-tabular file — one opaque "content" element).

`DataSource` and `DataAsset` are the two aggregate roots (independently
soft-deletable); `DiscoveryJob`/`DiscoveryResult`/`DataElement` are
children, cascade-deleted with their parent. Full schema facts in
`docs/DATABASE.md`.

## 7. FileSystem connector scope

Deliberately shallow for a first version: CSV/TSV files get real
column-level discovery (header row + up to 20 sample data lines, for
masking and an approximate row count — files over 5 MB skip the row-count
pass entirely rather than reading the whole file just to count lines).
Every other file type (PDF, DOCX, images, logs, ...) is recorded as one
asset with a single opaque `content` element — extension, size, and path
only, with **no attempt to parse or read the file's actual content**,
consistent with "do not implement advanced AI." Never follows symlinks or
reparse points, and re-validates every enumerated path stays under
`RootPath` as defense in depth (the same precedent
`FileSystemObjectStorageService`, Module 7, established for its own
storage root).

## 8. What is not covered by an automated test, and why

`MySqlDiscoveryConnector` and `SqlServerDiscoveryConnector` have no live
server in this development/CI environment, so they are not exercised by
an integration test the way `PostgresDiscoveryConnector` is (a real,
throwaway Postgres schema, created and torn down by
`DataDiscoveryApiFixture`, scanned end-to-end through the actual HTTP
API). Their SQL query shapes were written by the same pattern as the
Postgres connector (parameterized catalog-view queries, no string-built
predicates) and reviewed by inspection; a future environment with a real
MySQL/SQL Server instance available should add the equivalent live test.
