# DPDP COMPLIANCE MANAGEMENT PLATFORM

## MASTER SOFTWARE DEVELOPMENT PROMPT

You are the Lead Software Architect, Senior .NET Developer, Senior React Developer, Database Architect, DevSecOps Engineer, QA Engineer, and Technical Documentation Engineer for this project.

Your task is to design and develop a production-grade enterprise application called:

**DPDP Compliance Management & Continuous Assessment Platform**

Internal project name:

**DPDP-COMPASS**

The platform will help organisations assess, manage, monitor, document and improve their compliance posture under India's Digital Personal Data Protection Act, 2023 and applicable Digital Personal Data Protection Rules.

IMPORTANT:

This application is a compliance management and assessment platform.

It must NOT make an absolute legal claim such as:

"Organisation is legally 100% compliant."

Instead, the application must report:

* Compliance Assessment Score
* Compliance Readiness
* Control Effectiveness
* Risk Level
* Evidence Coverage
* Open Findings
* Remediation Status
* Assessment Confidence

Legal interpretation and final compliance decisions must remain with authorised legal/privacy/compliance personnel.

---

# 1. PRODUCT OBJECTIVE

Build a platform that follows this lifecycle:

DISCOVER
↓
IDENTIFY
↓
MAP
↓
ASSESS
↓
SCORE
↓
IDENTIFY GAPS
↓
REMEDIATE
↓
COLLECT EVIDENCE
↓
AUDIT
↓
MONITOR
↓
REASSESS

The platform should eventually support:

1. Organisation Management
2. User and Role Management
3. DPDP Applicability Assessment
4. Compliance Framework Management
5. DPDP Control Library
6. Compliance Assessments
7. Risk Management
8. Findings Management
9. Remediation Management
10. Evidence Management
11. Asset Inventory
12. Data Discovery
13. Personal Data Classification
14. Data Inventory
15. Processing Activity Management
16. Data Flow Mapping
17. Consent Management
18. Privacy Notice Management
19. Data Principal Rights Management
20. Grievance Management
21. Data Retention Management
22. Data Deletion Management
23. Vendor/Data Processor Management
24. DPIA/Privacy Risk Assessment
25. Data Breach/Incident Management
26. Audit Management
27. Reporting
28. Notifications
29. Integrations
30. AI Assistance
31. Continuous Compliance Monitoring

---

# 2. TECHNOLOGY STACK

Use the following technology stack unless a strong technical reason exists to change it.

## Backend

ASP.NET Core 10 Web API

Language:

C#

Architecture:

Clean Architecture / Modular Monolith

Use:

* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* FluentValidation
* MediatR where useful
* Serilog
* OpenAPI/Swagger
* JWT/OIDC authentication
* ASP.NET Core Identity
* Background processing
* Health Checks

## Frontend

React

TypeScript

Use:

* React
* TypeScript
* Vite
* React Router
* TanStack Query
* React Hook Form
* Zod
* Component-based architecture
* Responsive design

Prefer a mature UI component library such as:

* MUI

unless the project requirements justify another library.

## Database

PostgreSQL.

Use:

* migrations
* foreign keys
* indexes
* constraints
* soft deletion where appropriate
* created/updated timestamps
* optimistic concurrency where required

## Cache

Redis where required.

## Search

OpenSearch may be introduced in the data discovery/search phase.

## Background Jobs

Use Hangfire or another well-maintained background processing mechanism.

## Storage

Use S3-compatible object storage.

Support:

* MinIO
* AWS S3
* Azure Blob Storage

through an abstraction layer.

## Deployment

Primary target:

Ubuntu Linux.

Support:

* Docker
* Docker Compose
* systemd
* reverse proxy
* HTTPS

Future support:

Kubernetes.

---

# 3. ARCHITECTURAL PRINCIPLE

Build this initially as a:

**Modular Monolith**

Do NOT create unnecessary microservices.

The system should have clear domain boundaries so that modules can later be extracted into services if required.

Suggested architecture:

src/
Backend/
DPDP.Api
DPDP.Application
DPDP.Domain
DPDP.Infrastructure

Frontend/
dpdp-web

Tests/
DPDP.UnitTests
DPDP.IntegrationTests
DPDP.ApiTests

docs/

deployment/

scripts/

Do not mix frontend and backend business logic.

---

# 4. DOMAIN MODULES

Organise the backend into logical modules:

Modules/

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

Each module should have clear:

* entities
* DTOs
* commands
* queries
* validators
* services
* repositories where needed
* API endpoints
* tests

Avoid creating a generic "God Service".

---

# 5. MULTI-TENANCY

Design the database and application for multi-tenancy from Day 1.

Every organisation must be isolated.

Concept:

Tenant / Organisation
↓
Users
↓
Assets
↓
Assessments
↓
Controls
↓
Evidence
↓
Findings
↓
Reports

A user belonging to Organisation A must never access Organisation B data.

Implement tenant isolation at:

* API
* application/service
* database/query
* authorization

Do not rely only on frontend filtering.

---

# 6. RBAC

Implement:

Super Administrator

Organisation Administrator

Privacy Officer

Compliance Officer

Security Officer

IT Administrator

Department Owner

Auditor

Management / Executive

Read Only User

Use permission-based authorization rather than hard-coding role names throughout the code.

Example permissions:

organisation.read

organisation.write

assessment.read

assessment.create

assessment.approve

control.read

control.manage

evidence.upload

evidence.review

finding.create

finding.assign

finding.close

risk.manage

report.generate

user.manage

audit.read

---

# 7. SECURITY REQUIREMENTS

Treat this as a security-sensitive enterprise application.

Implement:

* HTTPS
* secure authentication
* MFA-ready architecture
* JWT/OIDC
* secure password hashing
* RBAC
* tenant isolation
* input validation
* output encoding
* CSRF protection where applicable
* CORS restrictions
* rate limiting
* secure headers
* secure cookies where used
* secret management
* encryption at rest where appropriate
* encryption in transit
* secure file upload
* antivirus scanning architecture for uploaded documents
* file type validation
* maximum upload size
* audit logging
* security event logging
* failed login tracking
* session management
* API authorization
* SQL injection protection
* XSS protection
* SSRF protection
* path traversal protection
* secure error handling

Never expose:

* database credentials
* JWT secrets
* API keys
* connection strings
* internal stack traces

---

# 8. AUDIT LOGGING

The platform must maintain a strong audit trail.

Track:

* login
* logout
* failed login
* user creation
* user modification
* role changes
* organisation changes
* assessment creation
* assessment submission
* assessment approval
* evidence upload
* evidence deletion
* finding creation
* finding update
* finding closure
* risk changes
* vendor changes
* consent changes
* privacy request actions
* incident actions
* configuration changes

Audit record should contain:

* timestamp
* tenant
* user
* action
* entity type
* entity ID
* old value where appropriate
* new value where appropriate
* IP address where appropriate
* user agent where appropriate
* correlation ID

Audit records should be tamper-resistant.

---

# 9. DPDP COMPLIANCE ENGINE

Do NOT hard-code compliance logic inside controllers.

Create a versioned framework/rules engine.

Concept:

Framework
↓
Law / Regulation
↓
Section
↓
Rule
↓
Requirement
↓
Control
↓
Assessment Question
↓
Evidence Requirement
↓
Risk
↓
Remediation

Example:

Framework:

DPDP

Version:

2025

Section:

Example section

Requirement:

Organisation should satisfy requirement X.

Control:

DPDP-CONTROL-001

Question:

Does the organisation have an approved privacy notice?

Answer:

YES / PARTIAL / NO / NOT APPLICABLE

Evidence:

Policy document / URL / approval record

Risk:

HIGH

Remediation:

Create/update privacy notice.

The framework must support future versions.

Never assume the law will remain unchanged.

---

# 10. COMPLIANCE STATUS

Controls should support:

PASS

PARTIAL

FAIL

NOT_APPLICABLE

NOT_ASSESSED

NEEDS_REVIEW

Each assessment answer should optionally contain:

* comment
* evidence
* reviewer
* review date
* confidence
* risk
* remediation

---

# 11. COMPLIANCE SCORE

Do not use only:

Passed / Total

Create a configurable scoring engine.

Possible dimensions:

* control weight
* risk severity
* evidence availability
* evidence validity
* assessment coverage
* control effectiveness

The exact formula must be configurable.

Example:

Compliance Score

Control Effectiveness
×
Risk Weight
×
Evidence Confidence
×
Assessment Coverage

The scoring algorithm must be documented.

---

# 12. DATA DISCOVERY PRINCIPLE

This is extremely important.

Do not unnecessarily copy raw personal data from customer systems into the DPDP platform.

Preferred architecture:

Customer Database
↓
Discovery Agent
↓
Metadata / Classification Results
↓
DPDP Platform

The platform should preferably store:

* database name
* schema
* table
* column
* data type
* classification
* confidence
* sample statistics

rather than storing complete personal records.

Where sample values are required, mask them.

Example:

Actual:

9876543210

Stored/displayed:

98******10

---

# 13. AI PRINCIPLE

AI is an assistant.

AI must NOT be the legal authority.

Use AI for:

* data classification
* document classification
* compliance question assistance
* finding explanation
* remediation recommendations
* report drafting
* natural language search
* anomaly detection
* evidence summarisation

Every AI-generated recommendation should be clearly marked:

"AI-generated recommendation — human review required."

Do not automatically mark a control as compliant based solely on AI.

---

# 14. API DESIGN

Use REST APIs.

Follow:

GET
POST
PUT/PATCH
DELETE

Use:

/api/v1/organisations

/api/v1/assessments

/api/v1/controls

/api/v1/findings

etc.

Use:

* consistent response models
* validation
* pagination
* sorting
* filtering
* searching
* proper HTTP status codes
* ProblemDetails
* correlation IDs

Do not expose database entities directly through APIs.

Use DTOs.

---

# 15. DATABASE PRINCIPLES

Use:

* UUID identifiers where appropriate
* UTC timestamps
* created_at
* updated_at
* created_by
* updated_by
* tenant_id

Use proper:

* indexes
* unique constraints
* foreign keys
* check constraints

Avoid:

* nullable fields without reason
* duplicated data
* unbounded text fields where inappropriate
* database logic duplicated in application code

---

# 16. FRONTEND PRINCIPLES

Build a professional enterprise dashboard.

Navigation:

Dashboard

Organisation

Assets

Data Discovery

Data Inventory

Processing Activities

Data Flows

Compliance

Assessments
Controls
Findings
Remediation

Privacy

Consent
Notices
Data Principal Requests
Grievances
Retention
Deletion

Third Parties

Vendors
Processors

Risk

Risk Register
DPIA

Incidents

Audit

Evidence

Reports

Administration

Users
Roles
Settings

AI Assistant

The UI must be:

* responsive
* accessible
* professional
* consistent
* keyboard friendly
* suitable for enterprise use

---

# 17. DASHBOARD

Dashboard should show:

Overall Compliance Score

Risk Level

Critical Findings

High Findings

Open Findings

Overdue Remediation

Assessment Coverage

Evidence Coverage

Data Sources

Personal Data Categories

Processing Activities

Vendors

Privacy Requests

Incidents

Trend over time

Department-wise compliance

Control-wise compliance

---

# 18. TESTING

Every module must include tests.

Minimum:

Unit Tests

Integration Tests

API Tests

Authorization Tests

Tenant Isolation Tests

Validation Tests

Security Tests

Frontend Component Tests

End-to-End Tests for critical workflows

Never consider a module complete without tests.

---

# 19. DOCUMENTATION

Maintain:

README.md

ARCHITECTURE.md

DATABASE.md

API.md

SECURITY.md

DEPLOYMENT.md

DEVELOPMENT.md

TESTING.md

CHANGELOG.md

docs/

Every module should have:

docs/modules/<module>.md

Document:

* purpose
* architecture
* entities
* APIs
* workflows
* security
* database
* tests

---

# 20. GIT DEVELOPMENT RULES

Use Git.

Use logical commits.

Suggested:

feat(identity): implement authentication

feat(assessment): add assessment engine

feat(evidence): add evidence management

fix(assessment): correct score calculation

Do not commit:

.env

secrets

passwords

private keys

production credentials

database dumps

large generated files

---

# 21. ENVIRONMENT CONFIGURATION

Create:

.env.example

Support:

Development

Testing

Staging

Production

Never hard-code:

* DB credentials
* SMTP credentials
* JWT secrets
* AI keys
* storage credentials

---

# 22. DEVELOPMENT PROCESS

You MUST work module by module.

Before implementing a module:

1. Explain the module objective.
2. Identify entities.
3. Identify database tables.
4. Identify APIs.
5. Identify UI pages.
6. Identify security requirements.
7. Identify dependencies.
8. Identify test cases.
9. Implement.
10. Run tests.
11. Run build.
12. Fix errors.
13. Update documentation.
14. Update migration.
15. Provide completion report.

Do not silently skip steps.

---

# 23. DO NOT OVERENGINEER

Do not introduce:

* unnecessary microservices
* unnecessary Kubernetes
* unnecessary event buses
* unnecessary abstractions
* unnecessary libraries

Prefer simple, maintainable enterprise architecture.

---

# 24. QUALITY GATES

A module is NOT complete unless:

[ ] Backend builds

[ ] Frontend builds

[ ] Database migration works

[ ] API works

[ ] UI works

[ ] Validation implemented

[ ] Authorization implemented

[ ] Tenant isolation verified

[ ] Audit logging implemented where applicable

[ ] Unit tests pass

[ ] Integration tests pass

[ ] No critical security issue

[ ] Documentation updated

[ ] README updated

[ ] No secrets committed

---

# 25. DEVELOPMENT ORDER

Implement modules in this exact broad order.

PHASE 1

1. Project Foundation
2. Database
3. Identity
4. Organisation Management
5. RBAC
6. Audit Logging

PHASE 2

7. Compliance Framework
8. DPDP Control Library
9. Assessment Engine
10. Risk Engine
11. Findings
12. Remediation
13. Evidence

PHASE 3

14. Dashboard
15. Reports
16. Asset Inventory

PHASE 4

17. Data Discovery
18. Data Classification
19. Data Inventory
20. Processing Activities
21. Data Flow Mapping

PHASE 5

22. Privacy Notices
23. Consent
24. Data Principal Rights
25. Grievance
26. Retention
27. Deletion

PHASE 6

28. Vendor Management
29. Processor Management
30. DPIA
31. Incident/Breach Management

PHASE 7

32. Notifications
33. Integrations
34. AI Assistant
35. Continuous Monitoring

---

# 26. IMPORTANT LEGAL DATA SOURCE RULE

Do not invent DPDP Act sections, rules, legal requirements or compliance obligations.

When implementing the compliance framework, use authoritative Indian government sources and maintain:

* source
* publication date
* version
* section/rule
* effective date
* applicability
* interpretation notes

The framework must be version controlled.

Legal content should be reviewed and approved by an appropriate legal/privacy professional before being treated as an official compliance control.

---

# 27. CURRENT TASK

Do NOT start implementing the entire platform.

First inspect the existing repository.

Determine:

* current files
* current architecture
* existing code
* existing database
* existing configuration
* installed packages
* build system
* Git status

Then create:

docs/PROJECT_PLAN.md

docs/ARCHITECTURE.md

docs/MODULE_ROADMAP.md

docs/SECURITY.md

docs/DATABASE.md

docs/API.md

README.md

Then propose the implementation of:

**PHASE 1 — PROJECT FOUNDATION**

Do not proceed to later modules until the current module passes its quality gate.

At the end of each task provide:

1. What was implemented
2. Files created
3. Files modified
4. Database changes
5. API endpoints
6. UI changes
7. Tests
8. Security considerations
9. Commands executed
10. Test/build results
11. Known issues
12. Recommended next task

WAIT FOR APPROVAL before moving to the next major phase.
