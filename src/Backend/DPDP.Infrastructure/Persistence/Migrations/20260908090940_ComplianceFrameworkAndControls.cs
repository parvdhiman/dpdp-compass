using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DPDP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceFrameworkAndControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliance_frameworks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    jurisdiction = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    issuing_authority = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compliance_frameworks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "control_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_control_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "compliance_framework_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    framework_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    official_citation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    publication_date = table.Column<DateOnly>(type: "date", nullable: true),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: true),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    review_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    change_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compliance_framework_versions", x => x.id);
                    table.ForeignKey(
                        name: "fk_compliance_framework_versions_compliance_frameworks_framewo",
                        column: x => x.framework_id,
                        principalTable: "compliance_frameworks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "controls",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    control_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    objective = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    control_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    risk_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    applicable_conditions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    evidence_requirements_summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    guidance = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    source_reference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: true),
                    review_date = table.Column<DateOnly>(type: "date", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    review_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_controls", x => x.id);
                    table.ForeignKey(
                        name: "fk_controls_control_categories_control_category_id",
                        column: x => x.control_category_id,
                        principalTable: "control_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliance_legal_references",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    framework_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    citation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    chapter = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    summary_text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    source_citation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    review_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compliance_legal_references", x => x.id);
                    table.ForeignKey(
                        name: "fk_compliance_legal_references_compliance_framework_versions_f",
                        column: x => x.framework_version_id,
                        principalTable: "compliance_framework_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assessment_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    control_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    help_text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    question_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    options_json = table.Column<string>(type: "jsonb", nullable: true),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assessment_questions", x => x.id);
                    table.ForeignKey(
                        name: "fk_assessment_questions_controls_control_id",
                        column: x => x.control_id,
                        principalTable: "controls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliance_requirements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    legal_reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    review_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compliance_requirements", x => x.id);
                    table.ForeignKey(
                        name: "fk_compliance_requirements_compliance_legal_references_legal_r",
                        column: x => x.legal_reference_id,
                        principalTable: "compliance_legal_references",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "evidence_requirements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    assessment_question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    acceptable_formats = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evidence_requirements", x => x.id);
                    table.ForeignKey(
                        name: "fk_evidence_requirements_assessment_questions_assessment_quest",
                        column: x => x.assessment_question_id,
                        principalTable: "assessment_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "control_mappings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    control_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requirement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mapping_notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_control_mappings", x => x.id);
                    table.ForeignKey(
                        name: "fk_control_mappings_controls_control_id",
                        column: x => x.control_id,
                        principalTable: "controls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_control_mappings_requirements_requirement_id",
                        column: x => x.requirement_id,
                        principalTable: "compliance_requirements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "compliance_frameworks",
                columns: new[] { "id", "code", "created_at", "description", "issuing_authority", "jurisdiction", "name" },
                values: new object[,]
                {
                    { new Guid("37081ea2-cec2-b8d7-9087-dde14457379e"), "DPDPA-2023", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "India's principal law governing the processing of digital personal data. Enacted to provide for the processing of digital personal data in a manner that recognises both the right of individuals to protect their personal data and the need to process such data for lawful purposes.", "Parliament of India", "India", "Digital Personal Data Protection Act, 2023" },
                    { new Guid("86694397-4b9f-43c5-64c3-6441933dff21"), "DPDPR-2025", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Subordinate legislation made under the rule-making power of the Digital Personal Data Protection Act, 2023, prescribing procedural and implementation detail. Detailed rule-by-rule content has not been added to this system pending verification against the final official Gazette notification — see docs/COMPLIANCE_CONTENT_GOVERNANCE.md.", "Ministry of Electronics and Information Technology, Government of India", "India", "Digital Personal Data Protection Rules, 2025" }
                });

            migrationBuilder.InsertData(
                table: "control_categories",
                columns: new[] { "id", "created_at", "description", "name", "sort_order" },
                values: new object[,]
                {
                    { new Guid("181c41f6-0d33-096e-f27e-753aa61e32e0"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Controls ensuring Data Principals receive proper notice and that consent is validly obtained, recorded, and withdrawable.", "Notice & Consent", 1 },
                    { new Guid("348021a0-9923-af29-e46c-ed0f53852151"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Controls specific to processing the personal data of children.", "Children's Data Protection", 5 },
                    { new Guid("440d7bbf-c44e-5e49-937f-05d0fb002a7d"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Additional governance controls applicable only to organisations notified as Significant Data Fiduciaries.", "Significant Data Fiduciary Governance", 6 },
                    { new Guid("72addd5e-a2f4-93c6-a380-92250cf7027d"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Controls covering an organisation's handling of Data Principal rights requests (access, correction, erasure, grievance redressal).", "Data Principal Rights", 7 },
                    { new Guid("752858d2-d595-6d18-bc99-bf87702e59a1"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Controls governing how long personal data is retained and how it is erased.", "Data Retention & Erasure", 3 },
                    { new Guid("9032e7f4-4067-3898-96a6-7ed8f0eedc29"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Controls covering technical/organisational security safeguards and personal data breach detection and notification.", "Security & Breach Management", 2 },
                    { new Guid("b9f1adc9-edb6-6c47-a8df-64e800833eed"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Controls covering general accountability obligations such as publishing contact details for Data Principal questions.", "Data Fiduciary Governance", 4 },
                    { new Guid("ce72e29c-ce22-650c-7771-1c6a1701b328"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Controls covering the transfer of personal data outside India.", "Cross-Border Data Transfer", 8 }
                });

            migrationBuilder.InsertData(
                table: "compliance_framework_versions",
                columns: new[] { "id", "change_summary", "created_at", "effective_date", "framework_id", "is_current", "official_citation", "publication_date", "review_status", "source_url", "version_label" },
                values: new object[,]
                {
                    { new Guid("22ead22c-c17c-4750-c33c-a1e6127fece3"), "Initial seed reflecting the enacted Act's chapter/section structure. Section-level summaries are plain-language paraphrases pending qualified legal review — not verbatim statutory text. Commencement of individual provisions is subject to phased notification by the Central Government under Section 1(2); confirm current commencement status against the official Gazette before treating any provision as legally binding on a specific date.", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("37081ea2-cec2-b8d7-9087-dde14457379e"), true, "Act No. 22 of 2023", new DateOnly(2023, 8, 11), "DRAFT", null, "2023" },
                    { new Guid("3c104a3e-31a0-b5ec-4f10-b8fd92ab48d5"), "Placeholder version created to demonstrate framework versioning capability. No legal references, requirements, or controls have been populated under this version — content will be added once verified against the final official Gazette notification of the Rules.", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("86694397-4b9f-43c5-64c3-6441933dff21"), true, null, null, "DRAFT", null, "2025" }
                });

            migrationBuilder.InsertData(
                table: "controls",
                columns: new[] { "id", "applicable_conditions", "control_category_id", "control_id", "created_at", "created_by", "description", "effective_date", "evidence_requirements_summary", "guidance", "name", "objective", "review_date", "review_status", "risk_level", "source_reference", "status", "updated_at", "updated_by", "version" },
                values: new object[,]
                {
                    { new Guid("0f49c905-41fb-48d9-8490-2467aae59f1c"), null, new Guid("9032e7f4-4067-3898-96a6-7ed8f0eedc29"), "DPDP-CTRL-004", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "The organisation implements technical and organisational measures proportionate to the volume and sensitivity of personal data it processes, to prevent unauthorised processing or accidental loss, destruction, or damage.", null, "Security policy documentation and/or independent audit/assessment reports.", "Consider encryption, access controls, logging/monitoring, employee training, and vendor security requirements proportionate to risk.", "Technical & Organisational Security Safeguards", "Protect personal data against breach through appropriate security controls.", null, "DRAFT", "CRITICAL", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 8(5)", "ACTIVE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1 },
                    { new Guid("33aea37e-bad7-dcfd-bacc-0f30f39fce4d"), null, new Guid("9032e7f4-4067-3898-96a6-7ed8f0eedc29"), "DPDP-CTRL-005", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "The organisation maintains a documented process to detect, assess, and notify the Data Protection Board of India and affected Data Principals of a personal data breach.", null, "Breach notification procedure/runbook document.", "The process should define detection, internal escalation, assessment, and notification steps and owners, without undue delay upon becoming aware of a breach.", "Personal Data Breach Notification Process", "Ensure timely, complete breach notification to the regulator and affected individuals.", null, "DRAFT", "CRITICAL", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 8(6)", "ACTIVE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1 },
                    { new Guid("71e57236-5441-f15c-aa6a-e9c61b642552"), null, new Guid("72addd5e-a2f4-93c6-a380-92250cf7027d"), "DPDP-CTRL-012", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "The organisation provides a readily accessible grievance redressal mechanism and responds to Data Principal grievances within a reasonable, published timeframe.", null, "Grievance redressal policy document and response-time commitment.", "Ensure the mechanism is genuinely accessible (e.g. a discoverable contact channel) and track resolution times.", "Data Principal Grievance Redressal Mechanism", "Give Data Principals an effective route to raise and resolve concerns before escalating to the Data Protection Board.", null, "DRAFT", "MEDIUM", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 13", "ACTIVE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1 },
                    { new Guid("73066b14-30e0-a7bc-6815-678b16522504"), "Applies only to organisations notified by the Central Government as a Significant Data Fiduciary.", new Guid("440d7bbf-c44e-5e49-937f-05d0fb002a7d"), "DPDP-CTRL-009", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "If notified as a Significant Data Fiduciary, the organisation appoints a Data Protection Officer based in India, engages an independent data auditor, and conducts periodic Data Protection Impact Assessments and audits.", null, "Most recent Data Protection Impact Assessment report and DPO/auditor appointment records.", "Confirm current Significant Data Fiduciary notification status before treating this control as applicable.", "Significant Data Fiduciary Governance Obligations", "Meet the heightened governance obligations applicable to Significant Data Fiduciaries.", null, "DRAFT", "HIGH", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 10", "ACTIVE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1 },
                    { new Guid("7d1352e9-e5a1-bd06-9885-5703c7d9f592"), null, new Guid("72addd5e-a2f4-93c6-a380-92250cf7027d"), "DPDP-CTRL-010", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "The organisation provides Data Principals a mechanism to request and receive a summary of their personal data being processed and related processing activities.", null, "Access request log or standard operating procedure with committed turnaround time.", "Publish a clear channel for access requests and a committed response timeframe.", "Data Principal Access Request Handling", "Enable Data Principals to exercise their right to access information about their own personal data.", null, "DRAFT", "MEDIUM", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 11", "ACTIVE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1 },
                    { new Guid("7ea3d3bf-5543-7390-6e62-64d2b2e3a68d"), "Applies only where processing relies on a Section 7 legitimate use rather than consent.", new Guid("181c41f6-0d33-096e-f27e-753aa61e32e0"), "DPDP-CTRL-003", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "Where personal data is processed without consent, the organisation documents which legitimate use ground under Section 7 applies and why.", null, "Written justification memo identifying the specific legitimate use ground relied upon.", "Document the specific sub-clause of Section 7 relied upon (e.g. voluntary provision of data, State function, medical emergency, employment purposes) and retain supporting rationale.", "Legitimate Use Documentation", "Demonstrate a lawful basis for processing that does not rely on consent.", null, "DRAFT", "MEDIUM", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 7", "ACTIVE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1 },
                    { new Guid("8c61e643-ce52-64c2-ca24-d9372f03e401"), "Applies whenever consent is the ground relied upon for processing.", new Guid("181c41f6-0d33-096e-f27e-753aa61e32e0"), "DPDP-CTRL-002", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "The organisation captures consent through clear affirmative action, limits it to the stated purpose, and provides an easy withdrawal mechanism.", null, "Consent capture UI/flow evidence and withdrawal mechanism evidence.", "Avoid bundled or pre-ticked consent. Provide a withdrawal path at least as easy as the original consent action.", "Consent Management Mechanism", "Ensure consent relied upon as a processing ground is valid, specific, and revocable.", null, "DRAFT", "HIGH", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 6", "ACTIVE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1 },
                    { new Guid("94fe7fc3-35f5-1e2f-c3f7-db32f7adaa36"), null, new Guid("b9f1adc9-edb6-6c47-a8df-64e800833eed"), "DPDP-CTRL-007", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "The organisation publishes the name and contact details of a Data Protection Officer or other person able to answer, on the organisation's behalf, questions about the processing of a Data Principal's personal data.", null, "Screenshot or URL of the published contact page.", "Publish this contact prominently, e.g. in the privacy notice and on the organisation's website.", "Grievance Redressal Contact Publication", "Give Data Principals an accessible point of contact for questions about their personal data.", null, "DRAFT", "MEDIUM", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 8(9)-(10)", "ACTIVE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1 },
                    { new Guid("a97ad5aa-449c-2f49-784a-c3f1e3b2b323"), "Applies whenever the organisation processes, or has reason to believe it processes, personal data of individuals below the age threshold defined in the Act and its rules.", new Guid("348021a0-9923-af29-e46c-ed0f53852151"), "DPDP-CTRL-008", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "The organisation obtains verifiable consent from a parent or lawful guardian before processing a child's personal data, and does not undertake tracking, behavioural monitoring, or targeted advertising directed at children.", null, "Parental/guardian consent verification workflow documentation.", "Confirm applicable exemptions (if any) before relying on them, and design age-assurance and parental consent flows conservatively.", "Children's Data Safeguards", "Protect children from processing likely to cause them harm.", null, "DRAFT", "CRITICAL", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 9", "ACTIVE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1 },
                    { new Guid("d34f72a6-24b3-eb8f-7224-1b6285e31f87"), "Applies whenever personal data is collected directly from a Data Principal with consent as the processing ground.", new Guid("181c41f6-0d33-096e-f27e-753aa61e32e0"), "DPDP-CTRL-001", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "The organisation provides Data Principals with a clear, itemised notice describing the personal data collected and the purpose(s) of processing before or at the time consent is requested.", null, "Notice template(s) or screenshots shown to Data Principals at the point of data collection.", "Notices should be available in plain language, itemise the categories of personal data collected and each purpose of processing, and describe how to exercise rights and lodge a grievance.", "Provide Notice Prior to Consent", "Ensure Data Principals are properly informed before their personal data is collected, enabling meaningful consent.", null, "DRAFT", "HIGH", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 5", "ACTIVE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1 },
                    { new Guid("e0b22388-7170-edf9-96c0-b22d93bea468"), null, new Guid("72addd5e-a2f4-93c6-a380-92250cf7027d"), "DPDP-CTRL-011", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "The organisation provides Data Principals a mechanism to request correction, completion, updating, and erasure of their personal data.", null, "Correction/erasure request handling SOP document.", "Define validation steps, response timeframes, and any lawful grounds for declining a request.", "Data Principal Correction & Erasure Request Handling", "Enable Data Principals to exercise their right to correction and erasure.", null, "DRAFT", "MEDIUM", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 12", "ACTIVE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1 },
                    { new Guid("f72110ec-0cf1-d959-eaa4-beb7f85944ef"), "Applies whenever the organisation transfers personal data to a recipient outside India.", new Guid("ce72e29c-ce22-650c-7771-1c6a1701b328"), "DPDP-CTRL-013", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "The organisation monitors and complies with any Central Government notification restricting transfer of personal data to specific countries or territories before transferring personal data outside India.", null, "Register of countries/territories to which personal data is transferred.", "Maintain a current transfer register and check it against any Central Government restriction notifications before each new cross-border transfer arrangement.", "Cross-Border Data Transfer Compliance Monitoring", "Avoid unlawful cross-border transfer of personal data.", null, "DRAFT", "HIGH", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 16", "ACTIVE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1 },
                    { new Guid("ffe6385f-9830-baf6-5024-7213b9488417"), null, new Guid("752858d2-d595-6d18-bc99-bf87702e59a1"), "DPDP-CTRL-006", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "The organisation erases personal data when the specified purpose is no longer being served or consent is withdrawn, unless retention is required by law, and applies the same requirement to its data processors.", null, "Data retention policy specifying retention periods by data/purpose category.", "Map retention periods to specific purposes and legal retention obligations; ensure processors are contractually bound to the same erasure requirements.", "Data Retention & Erasure Schedule", "Avoid retaining personal data beyond what is necessary or lawful.", null, "DRAFT", "HIGH", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 8(7)-(8)", "ACTIVE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 1 }
                });

            migrationBuilder.InsertData(
                table: "assessment_questions",
                columns: new[] { "id", "code", "control_id", "created_at", "deleted_at", "deleted_by", "help_text", "is_deleted", "is_required", "options_json", "question_type", "sort_order", "text" },
                values: new object[,]
                {
                    { new Guid("0cb0c824-6076-844e-ce0a-bf0799aa3179"), "Q-CTRL-008-1", new Guid("a97ad5aa-449c-2f49-784a-c3f1e3b2b323"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Answer NOT_APPLICABLE-equivalent by selecting No and explaining in follow-up evidence if the organisation does not process children's data at all.", false, true, null, "YES_NO", 8, "Does the organisation obtain verifiable parental or guardian consent before processing a child's personal data?" },
                    { new Guid("35cfcfdf-3ca7-f5bc-0f6d-588c78ae286b"), "Q-CTRL-003-1", new Guid("7ea3d3bf-5543-7390-6e62-64d2b2e3a68d"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Reference the specific Section 7 sub-clause relied upon.", false, true, null, "TEXT", 3, "Describe the legitimate use ground(s) relied upon and the documented justification." },
                    { new Guid("3ffdd422-63a8-a4b0-2d94-7abf64b63f5e"), "Q-CTRL-001-1", new Guid("d34f72a6-24b3-eb8f-7224-1b6285e31f87"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Notice should be in plain language and itemise categories of data and each purpose.", false, true, null, "YES_NO", 1, "Does the organisation provide an itemised notice to Data Principals describing the personal data collected and the purpose of processing, before or at the time of seeking consent?" },
                    { new Guid("449eb44f-37a3-c207-b72c-47a83c7ab3c0"), "Q-CTRL-005-1", new Guid("33aea37e-bad7-dcfd-bacc-0f30f39fce4d"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "A written runbook, not just an informal practice.", false, true, null, "YES_NO", 5, "Does the organisation have a documented process to notify the Data Protection Board of India and affected Data Principals of a personal data breach?" },
                    { new Guid("630891f9-ba4a-52e9-a2e2-f05cce342473"), "Q-CTRL-007-1", new Guid("94fe7fc3-35f5-1e2f-c3f7-db32f7adaa36"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "This is typically part of the privacy notice.", false, true, null, "URL", 7, "Provide the URL where the organisation publishes its Data Protection Officer / grievance contact details." },
                    { new Guid("7138ce20-953c-b0d9-69b2-73875a68d9c8"), "Q-CTRL-006-1", new Guid("ffe6385f-9830-baf6-5024-7213b9488417"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Enter the longest applicable retention period across data categories.", false, true, null, "NUMBER", 6, "What is the maximum retention period, in days, applied to personal data after the processing purpose is fulfilled, absent a legal retention requirement?" },
                    { new Guid("78577924-da28-1c66-e970-51fa492b27ff"), "Q-CTRL-009-1", new Guid("73066b14-30e0-a7bc-6815-678b16522504"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Only applicable if notified as a Significant Data Fiduciary.", false, true, null, "DATE", 9, "What was the date of the organisation's most recent Data Protection Impact Assessment?" },
                    { new Guid("9e763d21-ee77-f34a-17d5-2c8fb6213cbe"), "Q-CTRL-002-1", new Guid("8c61e643-ce52-64c2-ca24-d9372f03e401"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Select the option that best describes the current mechanism.", false, true, "[\"Explicit opt-in via clear affirmative action\",\"Pre-ticked or implied consent\",\"No formal consent mechanism\",\"Not applicable \\u2014 relies solely on a Section 7 legitimate use\"]", "MULTIPLE_CHOICE", 2, "How does the organisation capture consent from Data Principals?" },
                    { new Guid("a3c79a1c-ec76-f89b-60c6-6e77dc5c582b"), "Q-CTRL-012-1", new Guid("71e57236-5441-f15c-aa6a-e9c61b642552"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, false, true, null, "TEXT", 12, "Describe the organisation's grievance redressal process and its response-time commitment." },
                    { new Guid("a616930b-bc1c-3f9c-4c4c-dc209478fefd"), "Q-CTRL-004-1", new Guid("0f49c905-41fb-48d9-8490-2467aae59f1c"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Select all that apply.", false, true, "[\"Encryption at rest\",\"Encryption in transit\",\"Access controls / least privilege\",\"Regular security audits\",\"Employee security training\",\"Documented incident response plan\"]", "MULTI_SELECT", 4, "Which of the following technical/organisational safeguards are implemented?" },
                    { new Guid("b2990783-bb7b-6f5d-9340-979cb7fa663f"), "Q-CTRL-011-1", new Guid("e0b22388-7170-edf9-96c0-b22d93bea468"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, false, true, null, "YES_NO", 11, "Does the organisation provide a mechanism for Data Principals to request correction or erasure of their personal data?" },
                    { new Guid("f076759f-bdbe-9540-90c0-dd2e844e0476"), "Q-CTRL-010-1", new Guid("7d1352e9-e5a1-bd06-9885-5703c7d9f592"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Enter the published or internally committed SLA.", false, true, null, "NUMBER", 10, "What is the organisation's committed turnaround time, in days, for responding to a Data Principal access request?" },
                    { new Guid("fa05422c-173f-56e5-62aa-aef0eb415ef8"), "Q-CTRL-013-1", new Guid("f72110ec-0cf1-d959-eaa4-beb7f85944ef"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Upload 'none' documentation if no cross-border transfers occur.", false, true, null, "FILE", 13, "Upload the organisation's current register of countries/territories to which personal data is transferred, if any." }
                });

            migrationBuilder.InsertData(
                table: "compliance_legal_references",
                columns: new[] { "id", "chapter", "citation", "created_at", "framework_version_id", "review_status", "source_citation", "summary_text", "title" },
                values: new object[,]
                {
                    { new Guid("07431164-f6dd-04f8-d73b-3252c7e25fc1"), "Chapter III — Rights and Duties of Data Principal", "Section 11", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("22ead22c-c17c-4750-c33c-a1e6127fece3"), "DRAFT", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 11", "Gives a Data Principal the right to obtain from a Data Fiduciary a summary of personal data being processed and the processing activities undertaken, subject to exceptions set out in the Act.", "Right to access information about personal data" },
                    { new Guid("106950e2-d9b7-a54b-3228-4991da6ce6b9"), "Chapter II — Obligations of Data Fiduciary", "Section 5", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("22ead22c-c17c-4750-c33c-a1e6127fece3"), "DRAFT", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 5", "Requires a Data Fiduciary to give the Data Principal notice — before or at the time of requesting consent — describing the personal data to be collected and the purpose of processing, in clear and plain language, along with information on how to exercise rights and lodge complaints.", "Notice" },
                    { new Guid("396c887b-d17f-9288-ea42-dce6348b1c2a"), "Chapter II — Obligations of Data Fiduciary", "Section 7", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("22ead22c-c17c-4750-c33c-a1e6127fece3"), "DRAFT", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 7", "Lists specified situations in which a Data Fiduciary may process personal data without obtaining consent, such as where the Data Principal has voluntarily provided the data for a specified purpose, for the performance of functions of the State, medical emergencies, or employment-related purposes, among others set out in the Act.", "Certain legitimate uses" },
                    { new Guid("3978d2a8-13ad-3c70-8c4d-59a23166fbef"), "Chapter II — Obligations of Data Fiduciary", "Section 8", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("22ead22c-c17c-4750-c33c-a1e6127fece3"), "DRAFT", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 8", "Sets out the Data Fiduciary's general obligations, including maintaining accuracy of personal data, implementing reasonable security safeguards to prevent personal data breaches, notifying the Data Protection Board of India and affected Data Principals of a breach, erasing personal data when the purpose is no longer served (subject to legal retention requirements), and publishing the contact details of a person able to answer Data Principal questions about the processing of their personal data.", "General obligations of Data Fiduciary" },
                    { new Guid("67b99919-0906-5b68-2771-46577383831d"), "Chapter III — Rights and Duties of Data Principal", "Section 12", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("22ead22c-c17c-4750-c33c-a1e6127fece3"), "DRAFT", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 12", "Gives a Data Principal the right to request correction, completion, updating, and erasure of personal data, which the Data Fiduciary must act upon unless retention is necessary for a specified purpose or under law.", "Right to correction and erasure of personal data" },
                    { new Guid("7a6c42de-ce17-690e-0493-52ea6d63c1a3"), "Chapter IV — Special Provisions", "Section 16", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("22ead22c-c17c-4750-c33c-a1e6127fece3"), "DRAFT", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 16", "Empowers the Central Government to restrict, by notification, the transfer of personal data by a Data Fiduciary for processing to specific countries or territories outside India.", "Processing of personal data outside India" },
                    { new Guid("87cc11dd-34f4-7e27-692b-54d422b2849f"), "Chapter II — Obligations of Data Fiduciary", "Section 10", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("22ead22c-c17c-4750-c33c-a1e6127fece3"), "DRAFT", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 10", "Empowers the Central Government to notify certain Data Fiduciaries as Significant Data Fiduciaries based on factors such as the volume and sensitivity of personal data processed, and imposes additional obligations on them, including appointing a Data Protection Officer based in India, appointing an independent data auditor, and undertaking periodic Data Protection Impact Assessments and audits.", "Additional obligations of Significant Data Fiduciary" },
                    { new Guid("909d4e0f-1875-9e30-bee7-31973219b191"), "Chapter II — Obligations of Data Fiduciary", "Section 6", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("22ead22c-c17c-4750-c33c-a1e6127fece3"), "DRAFT", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 6", "Requires that consent be free, specific, informed, unconditional, and unambiguous, given through clear affirmative action, and limited to the personal data necessary for the specified purpose. A Data Principal must be able to withdraw consent at any time as easily as it was given.", "Consent" },
                    { new Guid("970a908e-4dda-c574-6a00-0b0b50216823"), "Chapter II — Obligations of Data Fiduciary", "Section 9", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("22ead22c-c17c-4750-c33c-a1e6127fece3"), "DRAFT", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 9", "Requires a Data Fiduciary to obtain verifiable consent of a parent or lawful guardian before processing the personal data of a child, and prohibits processing likely to cause a detrimental effect on a child's wellbeing or tracking, behavioural monitoring, or targeted advertising directed at children, subject to exemptions the Central Government may notify.", "Processing of personal data of children" },
                    { new Guid("bfcc94d5-f476-d51d-abd5-2243f75d5940"), "Chapter II — Obligations of Data Fiduciary", "Section 4", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("22ead22c-c17c-4750-c33c-a1e6127fece3"), "DRAFT", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 4", "Provides that a Data Fiduciary may process a Data Principal's personal data only in accordance with the Act, and only for a lawful purpose — either with the Data Principal's consent or for certain legitimate uses specified in the Act.", "Grounds for processing personal data" },
                    { new Guid("c91ca9fb-e559-9f8a-4fc5-3fbc62164ee0"), "Chapter III — Rights and Duties of Data Principal", "Section 13", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("22ead22c-c17c-4750-c33c-a1e6127fece3"), "DRAFT", "Digital Personal Data Protection Act, 2023 (Act No. 22 of 2023), Section 13", "Gives a Data Principal the right to have readily available means of grievance redressal provided by a Data Fiduciary or Consent Manager in respect of any act or omission regarding the performance of obligations under the Act.", "Right of grievance redressal" }
                });

            migrationBuilder.InsertData(
                table: "compliance_requirements",
                columns: new[] { "id", "code", "created_at", "description", "legal_reference_id", "review_status", "title" },
                values: new object[,]
                {
                    { new Guid("004e2bb4-aff9-d660-6ef6-2dec602a4f2b"), "REQ-CHILD-01", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Obtain verifiable consent from a parent or lawful guardian before processing a child's personal data, and do not undertake tracking, behavioural monitoring, or targeted advertising directed at children, unless a Central Government exemption applies.", new Guid("970a908e-4dda-c574-6a00-0b0b50216823"), "DRAFT", "Safeguard children's personal data" },
                    { new Guid("00a08c4b-9fbf-a1ab-f360-9f953a7f8c7a"), "REQ-CONTACT-01", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Publish the name and contact details of a Data Protection Officer or other person able to answer, on behalf of the Data Fiduciary, questions about the processing of a Data Principal's personal data.", new Guid("3978d2a8-13ad-3c70-8c4d-59a23166fbef"), "DRAFT", "Publish contact details for Data Principal questions" },
                    { new Guid("197146b3-0529-8a15-fc56-bd363c0bafd7"), "REQ-SDF-01", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "If notified as a Significant Data Fiduciary, appoint a Data Protection Officer based in India, engage an independent data auditor, and conduct periodic Data Protection Impact Assessments and audits.", new Guid("87cc11dd-34f4-7e27-692b-54d422b2849f"), "DRAFT", "Meet Significant Data Fiduciary obligations" },
                    { new Guid("1b0bed3f-806b-aca0-5331-2e40aff1ecf0"), "REQ-TRANSFER-01", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Monitor and comply with any Central Government notification restricting transfer of personal data to specific countries or territories, before transferring personal data outside India.", new Guid("7a6c42de-ce17-690e-0493-52ea6d63c1a3"), "DRAFT", "Monitor cross-border transfer restrictions" },
                    { new Guid("2624c3e3-33b0-371c-fee6-341e9311f39c"), "REQ-ERASURE-01", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Erase personal data upon withdrawal of consent or as soon as the specified purpose is no longer being served, unless retention is required by law, and require the same of any data processor engaged.", new Guid("3978d2a8-13ad-3c70-8c4d-59a23166fbef"), "DRAFT", "Erase personal data when no longer needed" },
                    { new Guid("5eae1c0c-0741-77ca-a47f-b5b11fc1fced"), "REQ-CONSENT-01", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Ensure that consent is captured through clear affirmative action, is specific to the stated purpose, and is not bundled with unrelated purposes. Provide a mechanism for a Data Principal to withdraw consent at any time, at least as easily as it was given, and stop processing (subject to legal exceptions) upon withdrawal.", new Guid("909d4e0f-1875-9e30-bee7-31973219b191"), "DRAFT", "Operate a valid consent mechanism" },
                    { new Guid("8d7429bd-04fe-5fd7-776d-b5760af11ddb"), "REQ-CORRECT-01", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Provide Data Principals a mechanism to request correction, completion, updating, and erasure of their personal data, and act on valid requests unless a lawful ground for continued retention exists.", new Guid("67b99919-0906-5b68-2771-46577383831d"), "DRAFT", "Respond to correction and erasure requests" },
                    { new Guid("b8e59597-dc3a-a52b-c085-3892b3161e2c"), "REQ-GRIEVANCE-01", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Provide a readily accessible grievance redressal mechanism, publish the contact details of a person able to address Data Principal questions and complaints, and respond within a reasonable, published timeframe.", new Guid("c91ca9fb-e559-9f8a-4fc5-3fbc62164ee0"), "DRAFT", "Operate a grievance redressal mechanism" },
                    { new Guid("d212a9e6-85d2-3dc9-9959-0b0fb793fa52"), "REQ-BREACH-01", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Maintain a documented process to detect, assess, and notify the Data Protection Board of India and affected Data Principals of a personal data breach without undue delay.", new Guid("3978d2a8-13ad-3c70-8c4d-59a23166fbef"), "DRAFT", "Notify personal data breaches" },
                    { new Guid("e3337727-edad-fab8-2114-e32e3eb69169"), "REQ-ACCESS-01", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Provide Data Principals a mechanism to request and receive a summary of their personal data being processed and the related processing activities, within a reasonable, published timeframe.", new Guid("07431164-f6dd-04f8-d73b-3252c7e25fc1"), "DRAFT", "Respond to access requests" },
                    { new Guid("ea47c405-3ee1-1467-36f5-1a639fe33cfb"), "REQ-NOTICE-01", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Provide Data Principals with a clear, itemised notice — describing the personal data collected and the purpose(s) of processing, in plain language — before or at the time consent is sought, and make available the means to exercise rights under the Act and to lodge a complaint with the Data Protection Board.", new Guid("106950e2-d9b7-a54b-3228-4991da6ce6b9"), "DRAFT", "Provide itemised notice before or at consent" },
                    { new Guid("eea09958-2696-a2fe-4fa5-c880a8ca6992"), "REQ-LEGITUSE-01", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Where personal data is processed without consent in reliance on a legitimate use under Section 7, document and be able to demonstrate which specific ground is relied upon and why it applies.", new Guid("396c887b-d17f-9288-ea42-dce6348b1c2a"), "DRAFT", "Document reliance on a legitimate use ground" },
                    { new Guid("ff22afc2-8ede-b41f-6e7d-9a01c95c8e5b"), "REQ-SEC-01", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Implement appropriate technical and organisational measures to protect personal data against unauthorised processing, accidental loss, destruction, or damage — proportionate to the volume and sensitivity of personal data processed.", new Guid("3978d2a8-13ad-3c70-8c4d-59a23166fbef"), "DRAFT", "Implement reasonable security safeguards" }
                });

            migrationBuilder.InsertData(
                table: "evidence_requirements",
                columns: new[] { "id", "acceptable_formats", "assessment_question_id", "created_at", "deleted_at", "deleted_by", "description", "is_deleted", "is_mandatory", "name" },
                values: new object[,]
                {
                    { new Guid("01fe65e7-8973-ad61-03f6-6e2f84f0eb71"), "PDF, DOCX, XLSX", new Guid("fa05422c-173f-56e5-62aa-aef0eb415ef8"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Register/list of countries or territories to which personal data is transferred.", false, false, "Cross-border transfer register" },
                    { new Guid("02db6d97-dc86-df54-d452-4319d5ec4728"), "PDF, DOCX", new Guid("7138ce20-953c-b0d9-69b2-73875a68d9c8"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Data retention policy document specifying retention periods by data/purpose category.", false, true, "Data retention policy" },
                    { new Guid("091fde5b-d852-f58b-2950-2446ce98693d"), "PDF, DOCX", new Guid("78577924-da28-1c66-e970-51fa492b27ff"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Most recent Data Protection Impact Assessment report.", false, true, "DPIA report" },
                    { new Guid("138ff6f2-70a7-48c3-617e-0ca0bfecb051"), "PDF, DOCX", new Guid("35cfcfdf-3ca7-f5bc-0f6d-588c78ae286b"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Written memo identifying the specific Section 7 ground relied upon and supporting rationale.", false, true, "Legitimate use justification memo" },
                    { new Guid("188476dc-086d-d2d8-3331-8070ca1b56a2"), "PDF, PNG, JPG", new Guid("9e763d21-ee77-f34a-17d5-2c8fb6213cbe"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Screenshot or export of the consent capture flow/UI, including the withdrawal mechanism.", false, true, "Consent capture evidence" },
                    { new Guid("388650d9-cd99-f761-4dba-6a1528f1db33"), "PDF, DOCX", new Guid("a3c79a1c-ec76-f89b-60c6-6e77dc5c582b"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Grievance redressal policy document, including response-time commitments.", false, true, "Grievance redressal policy" },
                    { new Guid("479928a3-d628-80fd-3e0c-f6dc40da8c1a"), "PDF, DOCX", new Guid("0cb0c824-6076-844e-ce0a-bf0799aa3179"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Documentation of the parental/guardian consent verification workflow.", false, true, "Parental consent workflow evidence" },
                    { new Guid("54fdaef0-0fbc-573a-58ce-591edc603f3e"), "PDF, DOCX", new Guid("449eb44f-37a3-c207-b72c-47a83c7ab3c0"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Documented breach detection, escalation, and notification procedure/runbook.", false, true, "Breach notification procedure" },
                    { new Guid("7923c01f-fcb5-5474-f0d2-0570d1826905"), "PDF, DOCX, PNG, JPG", new Guid("3ffdd422-63a8-a4b0-2d94-7abf64b63f5e"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Copy of the notice template/document or screenshot shown to Data Principals at the point of data collection.", false, true, "Notice template or evidence" },
                    { new Guid("7b321b73-db4a-7bc6-cc9a-8e88dc1bd00c"), "PDF, DOCX, XLSX", new Guid("f076759f-bdbe-9540-90c0-dd2e844e0476"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Access request log or standard operating procedure document showing committed turnaround times.", false, false, "Access request handling evidence" },
                    { new Guid("8d7bd9c3-c83d-ae49-c3a4-fdfb7f871b1f"), "PDF, DOCX", new Guid("a616930b-bc1c-3f9c-4c4c-dc209478fefd"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Security policy document and/or independent audit or assessment report evidencing implemented safeguards.", false, true, "Security policy / audit evidence" },
                    { new Guid("bf5bd228-d755-99fe-42a5-9eb35d362208"), "PDF, DOCX", new Guid("b2990783-bb7b-6f5d-9340-979cb7fa663f"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Correction/erasure request handling standard operating procedure document.", false, true, "Correction/erasure request SOP" },
                    { new Guid("d5a2a13e-ce0f-fc24-3005-cf34c670864e"), "PDF, PNG, JPG", new Guid("630891f9-ba4a-52e9-a2e2-f05cce342473"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Screenshot or URL of the published DPO/grievance contact page.", false, true, "Published contact page evidence" }
                });

            migrationBuilder.InsertData(
                table: "control_mappings",
                columns: new[] { "id", "control_id", "created_at", "mapping_notes", "requirement_id" },
                values: new object[,]
                {
                    { new Guid("0af65cd0-64da-d383-ee0a-677beb1a1607"), new Guid("d34f72a6-24b3-eb8f-7224-1b6285e31f87"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("ea47c405-3ee1-1467-36f5-1a639fe33cfb") },
                    { new Guid("39c2ea11-ee5a-68b3-fa2c-7d7020b48b8b"), new Guid("94fe7fc3-35f5-1e2f-c3f7-db32f7adaa36"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("00a08c4b-9fbf-a1ab-f360-9f953a7f8c7a") },
                    { new Guid("43d6d739-3df7-ca6b-992b-299d0951f3b2"), new Guid("33aea37e-bad7-dcfd-bacc-0f30f39fce4d"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("d212a9e6-85d2-3dc9-9959-0b0fb793fa52") },
                    { new Guid("5aaf6cf2-c3f4-12f6-3c28-08b86bbfa57c"), new Guid("a97ad5aa-449c-2f49-784a-c3f1e3b2b323"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("004e2bb4-aff9-d660-6ef6-2dec602a4f2b") },
                    { new Guid("5daefbc0-68a7-3f7e-283d-2ddd8865813d"), new Guid("7ea3d3bf-5543-7390-6e62-64d2b2e3a68d"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("eea09958-2696-a2fe-4fa5-c880a8ca6992") },
                    { new Guid("7be38910-aa60-9484-1f78-c71b654a143d"), new Guid("f72110ec-0cf1-d959-eaa4-beb7f85944ef"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("1b0bed3f-806b-aca0-5331-2e40aff1ecf0") },
                    { new Guid("858b1072-6ddc-cca2-71ee-1f2be0b4e048"), new Guid("e0b22388-7170-edf9-96c0-b22d93bea468"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("8d7429bd-04fe-5fd7-776d-b5760af11ddb") },
                    { new Guid("995b0a13-ed27-a1f4-fe93-269fba2e2093"), new Guid("8c61e643-ce52-64c2-ca24-d9372f03e401"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("5eae1c0c-0741-77ca-a47f-b5b11fc1fced") },
                    { new Guid("bea1ef26-d13f-1ea3-5fbd-099b58ef90fa"), new Guid("71e57236-5441-f15c-aa6a-e9c61b642552"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("b8e59597-dc3a-a52b-c085-3892b3161e2c") },
                    { new Guid("dae2a128-930c-0bd4-531b-a96aa12300ca"), new Guid("0f49c905-41fb-48d9-8490-2467aae59f1c"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("ff22afc2-8ede-b41f-6e7d-9a01c95c8e5b") },
                    { new Guid("db76415c-073f-2058-4b33-0e10169f9f98"), new Guid("7d1352e9-e5a1-bd06-9885-5703c7d9f592"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("e3337727-edad-fab8-2114-e32e3eb69169") },
                    { new Guid("e567d3d6-d668-00c5-96f3-a5283493a9d1"), new Guid("73066b14-30e0-a7bc-6815-678b16522504"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("197146b3-0529-8a15-fc56-bd363c0bafd7") },
                    { new Guid("f54f2ebd-e3a2-2c2a-90be-dbd21ea6f749"), new Guid("ffe6385f-9830-baf6-5024-7213b9488417"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, new Guid("2624c3e3-33b0-371c-fee6-341e9311f39c") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_assessment_questions_code",
                table: "assessment_questions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assessment_questions_control_id",
                table: "assessment_questions",
                column: "control_id");

            migrationBuilder.CreateIndex(
                name: "ix_compliance_framework_versions_framework_id_version_label",
                table: "compliance_framework_versions",
                columns: new[] { "framework_id", "version_label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_compliance_frameworks_code",
                table: "compliance_frameworks",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_compliance_legal_references_framework_version_id_citation",
                table: "compliance_legal_references",
                columns: new[] { "framework_version_id", "citation" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_compliance_requirements_code",
                table: "compliance_requirements",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_compliance_requirements_legal_reference_id",
                table: "compliance_requirements",
                column: "legal_reference_id");

            migrationBuilder.CreateIndex(
                name: "ix_control_categories_name",
                table: "control_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_control_mappings_control_id_requirement_id",
                table: "control_mappings",
                columns: new[] { "control_id", "requirement_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_control_mappings_requirement_id",
                table: "control_mappings",
                column: "requirement_id");

            migrationBuilder.CreateIndex(
                name: "ix_controls_control_category_id",
                table: "controls",
                column: "control_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_controls_control_id",
                table: "controls",
                column: "control_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_controls_risk_level",
                table: "controls",
                column: "risk_level");

            migrationBuilder.CreateIndex(
                name: "ix_controls_status",
                table: "controls",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_evidence_requirements_assessment_question_id",
                table: "evidence_requirements",
                column: "assessment_question_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "control_mappings");

            migrationBuilder.DropTable(
                name: "evidence_requirements");

            migrationBuilder.DropTable(
                name: "compliance_requirements");

            migrationBuilder.DropTable(
                name: "assessment_questions");

            migrationBuilder.DropTable(
                name: "compliance_legal_references");

            migrationBuilder.DropTable(
                name: "controls");

            migrationBuilder.DropTable(
                name: "compliance_framework_versions");

            migrationBuilder.DropTable(
                name: "control_categories");

            migrationBuilder.DropTable(
                name: "compliance_frameworks");
        }
    }
}
