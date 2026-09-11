# DPDP-COMPASS — Module Roadmap

Every module listed here follows the 13-step development loop in
MASTER_PROMPT §22 and is not considered complete until it passes the
quality gate in §24 (see `PROJECT_PLAN.md` §5 for the Phase 1 instance of
that gate). Status is updated as each module ships; nothing below Phase 1
starts before Phase 1's gate is confirmed passed.

| # | Module | Phase | Key Entities (indicative) | Depends On | Status |
|---|---|---|---|---|---|
| 1 | Project Foundation | 1 | — (solution, CI, docker-compose, env config) | — | **Gate Passed** |
| 2 | Database | 1 | (cross-cutting: `DpdpDbContext`, migration baseline) | Foundation | **Gate Passed** |
| 3 | Identity | 1 | User, RefreshToken, PasswordResetToken, LoginHistory | Database | **Gate Passed** |
| 4 | Organisation Management | 1 | Organisation, OrganisationLocation, BusinessUnit, Department | Identity | **Gate Passed** — full CRUD, profile (industry/size/country/website/contacts/DPO/locations), business unit & department hierarchy, dashboard |
| 5 | RBAC | 1 | Role, Permission, RolePermission, UserRole | Identity, Organisation | **Gate Passed** |
| 6 | Audit Logging | 1 | AuditLog | Identity, Organisation | **Gate Passed** — write path only; no `GET /audit-logs` endpoint yet |
| 7 | Compliance Framework | 2 | Framework, FrameworkVersion, LegalReference, Requirement | RBAC, Audit | **Gate Passed** — built together with Module 8 as "Module 4"; see `docs/ARCHITECTURE.md` §14 |
| 8 | DPDP Control Library | 2 | Control, ControlCategory, ControlMapping, AssessmentQuestion, EvidenceRequirement | Compliance Framework | **Gate Passed** — 13 DPDP Act 2023 controls seeded (`ACTIVE`/`DRAFT` review status); DPDP Rules, 2025 framework seeded as an intentionally empty shell — see `docs/COMPLIANCE_CONTENT_GOVERNANCE.md` |
| 9 | Assessment Engine | 2 | Assessment, AssessmentScope, AssessmentControl, AssessmentControlQuestion, AssessmentAnswer, AssessmentReview, AssessmentApproval | Control Library | **Gate Passed** — full DRAFT→IN_PROGRESS→SUBMITTED→UNDER_REVIEW→APPROVED/REJECTED→ARCHIVED workflow, configurable scoring engine (`docs/SCORING.md`); "AssessmentAnswer" and "ScoringConfig" implemented as `AssessmentControlQuestion`+`AssessmentAnswer` and `ScoringOptions`+`IComplianceScoringStrategy` respectively — see `docs/ARCHITECTURE.md` §15 |
| 10 | Risk Engine | 2 | Risk | Assessment Engine | **Gate Passed** — built together with Findings/Remediation as "Module 6"; configurable Likelihood/Impact/Data Sensitivity/Exposure methodology, see `docs/RISK_METHODOLOGY.md` |
| 11 | Findings | 2 | Finding | Assessment Engine, Risk Engine | **Gate Passed** — see `docs/ARCHITECTURE.md` §16 |
| 12 | Remediation | 2 | RemediationTask, RemediationComment | Findings | **Gate Passed** — task/owner/due date/status/comments/evidence/verification per the brief |
| 13 | Evidence | 2 | EvidenceItem, EvidenceVersion, EvidenceReviewRecord | Findings, Object Storage (Foundation) | **Gate Passed** — built as "Module 7"; upload/version/review lifecycle, file-type + magic-byte validation, checksum, malware-scan abstraction, object-storage abstraction (local filesystem today), see `docs/EVIDENCE_STORAGE.md`; "Evidence"/"EvidenceFile" implemented as `EvidenceItem`+`EvidenceVersion` (an item's file content is versioned, not a separate always-current file record) — see `docs/ARCHITECTURE.md` §17 |
| 14 | Dashboard | 3 | (read models over Phase 2 data) | Assessment/Risk/Findings/Evidence | Not started |
| 15 | Reports | 3 | ReportTemplate, ReportRun | Dashboard data sources | Not started |
| 16 | Asset Inventory | 3 | Asset | Organisation | Not started |
| 17 | Data Discovery | 4 | DataSource, DiscoveryJob, DiscoveryResult, DataAsset, DataElement | Organisation (no Asset Inventory dependency after all — see note) | **Gate Passed** — built together with Data Classification as "Module 8"; connector/agent model (PostgreSQL/MySQL/SQL Server/File System), metadata-only discovery with masked samples, background job processing, see `docs/DATA_DISCOVERY.md` |
| 18 | Data Classification | 4 | (folded into `DataElement.ClassificationCategory`/`.ClassificationConfidence`/`.ClassificationSource`) | Data Discovery | **Gate Passed** — built together with Data Discovery as "Module 8"; rule-based classifier (11 categories), human correction, see `docs/DATA_DISCOVERY.md` §4 |
| 19 | Data Inventory | 4 | DataInventoryItem, DataCategory, ItSystem, DataCollectionSource, Processor, Recipient, RetentionPolicy | Data Discovery/Classification | **Gate Passed** — built together with Processing Activities and Data Flow Mapping as "Module 9"; "System"/"Data Source" renamed `ItSystem`/`DataCollectionSource` to avoid collisions with Module 8, see `docs/DATA_INVENTORY.md` §1 |
| 20 | Processing Activities | 4 | ProcessingActivity | Data Inventory | **Gate Passed** — built together with Data Inventory as "Module 9"; DRAFT→IN_REVIEW→APPROVED→ARCHIVED workflow with separation of duties, CSV export, see `docs/DATA_INVENTORY.md` §3 |
| 21 | Data Flow Mapping | 4 | DataFlow | Processing Activities | **Gate Passed** — built together with Data Inventory as "Module 9"; metadata only, never an actual data pipe, see `docs/DATA_INVENTORY.md` §5 |
| 22 | Privacy Notices | 5 | PrivacyNotice | Processing Activities | **Gate Passed** — built together with Consent, Data Principal Rights, and Grievance as "Module 10"; "NoticeVersion" folded into `PrivacyNotice.Version` — one row per version, no separate child entity — see `docs/CONSENT_PRIVACY_OPERATIONS.md` §2 |
| 23 | Consent | 5 | ConsentRecord, ConsentPurpose, DataPrincipal | Privacy Notices | **Gate Passed** — built together with Privacy Notices as "Module 10"; `ConsentRecord` is compliance evidence with no delete endpoint, `DataPrincipal` stores only an external reference ID, see `docs/CONSENT_PRIVACY_OPERATIONS.md` §1, §3 |
| 24 | Data Principal Rights | 5 | DataPrincipalRequest, SlaPolicy | Consent | **Gate Passed** — built together with Privacy Notices as "Module 10"; SLA due dates come from a fully configurable `SlaPolicy` catalog, never a hard-coded day-count, see `docs/CONSENT_PRIVACY_OPERATIONS.md` §5 |
| 25 | Grievance | 5 | (folded into `DataPrincipalRequest.RequestType = GRIEVANCE`) | DPR Requests | **Gate Passed** — built together with Data Principal Rights as "Module 10"; identical field shape and workflow, see `docs/CONSENT_PRIVACY_OPERATIONS.md` §4 |
| 26 | Retention | 5 | RetentionSchedule (execution/scheduling only — `RetentionPolicy` itself already exists as of Module 9) | Data Inventory | Not started |
| 27 | Deletion | 5 | DeletionRequest, DeletionJob | Retention | Not started |
| 28 | Vendor Management | 6 | Vendor | Organisation | Not started |
| 29 | Processor Management | 6 | DPA (agreement) — a lightweight `Processor` entity already exists as of Module 9, see `docs/DATA_INVENTORY.md` | Vendor Management | Not started |
| 30 | DPIA | 6 | DPIA, DPIAAnswer | Processing Activities, Risk Engine | Not started |
| 31 | Incident/Breach Management | 6 | Incident, BreachNotification | Findings, Vendor Management | Not started |
| 32 | Notifications | 7 | NotificationRule, NotificationLog | All Phase 1–6 event sources | Not started |
| 33 | Integrations | 7 | IntegrationConnection, WebhookSubscription | Foundation (storage/secrets) | Not started |
| 34 | AI Assistant | 7 | AiQuery, AiRecommendation (always human-review-flagged) | Data Discovery, Findings, Reports | Not started |
| 35 | Continuous Monitoring | 7 | MonitoringRule, MonitoringEvent | Assessment Engine, Notifications | Not started |

## Notes on cross-cutting modules

- **Reports, Notifications, Integrations, AI** are consumers of other
  modules' data rather than isolated silos — they are scheduled late
  precisely because they need Phase 2–6 data sources to exist first, but
  their *interfaces* (e.g. `INotificationSender`, `IAiAssistant`) are
  stubbed in Foundation so later modules can depend on the abstraction
  without a retrofit.
- **Audit Logging** (Phase 1) is deliberately built early because every
  later module's completion report requires audit events for its own
  actions (MASTER_PROMPT §8 lists user/role/org changes *and*
  assessment/evidence/finding/risk/vendor/consent/privacy-request/incident/
  configuration actions) — the sink must exist before those events start
  firing in Phase 2+.
- **Object storage abstraction** (`IObjectStorage`) is built in Foundation
  even though the first real consumer (Evidence) isn't until Phase 2, so
  Evidence doesn't have to design storage from scratch under module-by-
  module time pressure.
- No module in Phase 4 (Data Discovery/Classification/Inventory) stores bulk
  personal data — see `ARCHITECTURE.md` §6 and MASTER_PROMPT §12; entities
  there are metadata/classification/masked-sample records referencing an
  external customer data source, not personal-data replicas.
- **Data Discovery's originally-planned dependency on Asset Inventory
  (item 16) turned out not to be real.** Discovery connects directly to a
  customer's own database/file-system endpoints via `DataSource` —
  it never needed an internal Asset Inventory to exist first. When Asset
  Inventory is eventually built, it is more likely to *consume*
  `DataAsset`/`DataElement` (a discovered table is itself an "asset") than
  the other way around; that relationship should be revisited when item
  16 is scheduled, not treated as settled by this note.

## Legend

- **Proposed** — planned in this document set, implementation not yet
  approved/started.
- **In Progress** — approved and under active development.
- **Gate Passed** — quality gate (MASTER_PROMPT §24) confirmed met.
- **Not started** — later phase, not yet planned in detail.
