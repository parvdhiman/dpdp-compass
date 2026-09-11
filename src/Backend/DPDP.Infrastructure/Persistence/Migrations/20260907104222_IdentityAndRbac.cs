using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DPDP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IdentityAndRbac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    old_value = table.Column<string>(type: "jsonb", nullable: true),
                    new_value = table.Column<string>(type: "jsonb", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "login_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    attempted_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_login_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organisations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    legal_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_system_role = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    security_stamp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false),
                    mfa_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    mfa_secret = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    failed_login_count = table.Column<int>(type: "integer", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_password_reset_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_password_reset_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "created_at", "description", "key", "module" },
                values: new object[,]
                {
                    { new Guid("025496a9-f552-36a2-4439-70e5d7732dd8"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Generate reports.", "reports.generate", "Reports" },
                    { new Guid("1aee2fe5-144e-d283-2f85-761b8641974e"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Activate or deactivate user accounts.", "users.disable", "Identity" },
                    { new Guid("1dd266e6-943a-c8e6-439e-c591fadbfe47"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Manage the DPDP control library.", "controls.manage", "Controls" },
                    { new Guid("270b3031-1aa9-3b41-9a31-af214c8a2ce8"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Create users.", "users.create", "Identity" },
                    { new Guid("52e1133d-e4e1-d3f4-af93-6ec8397d590f"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Edit user profile details.", "users.update", "Identity" },
                    { new Guid("533af64f-df74-7a52-8af7-e511db0e93d4"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View findings.", "findings.read", "Findings" },
                    { new Guid("56f53b5e-cc9d-a5ab-a530-156b8e41338b"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Manage the risk register.", "risks.manage", "Risk" },
                    { new Guid("5b1cabb3-30a0-dc0c-bc2d-c160cf86c6ee"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Upload evidence.", "evidence.upload", "Evidence" },
                    { new Guid("5cae153a-0854-ee73-1a78-b7053d97b7b3"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View roles and their permissions.", "roles.read", "Identity" },
                    { new Guid("5e1669e0-3f9e-22eb-841f-68cac453d67e"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Manage remediation tasks.", "remediation.manage", "Remediation" },
                    { new Guid("67227c72-d6b2-f692-36f8-77d2928cc300"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View the risk register.", "risks.read", "Risk" },
                    { new Guid("6caa7dbb-db5e-7f31-7c2a-82eecc185418"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Create findings.", "findings.create", "Findings" },
                    { new Guid("8f668761-f357-5e37-cc7f-65980d2c4adb"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View evidence.", "evidence.read", "Evidence" },
                    { new Guid("988207d4-1001-ac73-1c9e-4ad2aee51034"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assign findings.", "findings.assign", "Findings" },
                    { new Guid("9a82dbe9-a3ad-f54c-6b31-862958cd736e"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Edit organisation details.", "organisation.write", "Organisations" },
                    { new Guid("a5acab85-c910-8b1c-d3b0-b2b8ccbcd1fb"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View organisation details.", "organisation.read", "Organisations" },
                    { new Guid("a8027187-eae2-da2c-06f5-b78ea7161fc2"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View the DPDP control library.", "controls.read", "Controls" },
                    { new Guid("b34c0975-ac31-2467-c654-0138fd16d87c"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View remediation tasks.", "remediation.read", "Remediation" },
                    { new Guid("bf83dd9d-0066-7e7b-ebcd-97d308590c37"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Create compliance assessments.", "assessments.create", "Assessments" },
                    { new Guid("c4c4da74-fa5e-e2e6-cf26-1e2480932e79"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Review and approve evidence.", "evidence.review", "Evidence" },
                    { new Guid("cc9f6165-e27e-7892-b1ac-ea069cfa364d"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View users.", "users.read", "Identity" },
                    { new Guid("cdaa5381-1bf1-cdce-f39c-8e77089e77b7"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View the audit trail.", "audit.read", "Audit" },
                    { new Guid("e9d6c2e4-4c7a-9623-4c5d-ea01f69902da"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assign roles to users.", "roles.manage", "Identity" },
                    { new Guid("ee6179a3-5935-9799-56b8-84d64c94b72f"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View compliance assessments.", "assessments.read", "Assessments" },
                    { new Guid("f1aed973-70b5-2e78-4153-0f4a6f1be34f"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View reports.", "reports.read", "Reports" },
                    { new Guid("f73a3e44-9927-3d6a-4988-aa89192bc7a4"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Close findings.", "findings.close", "Findings" },
                    { new Guid("fbdadc99-1531-a501-16d0-21e226fa2805"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Approve compliance assessments.", "assessments.approve", "Assessments" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "created_at", "description", "is_system_role", "name" },
                values: new object[,]
                {
                    { new Guid("016c7d6f-109e-eed9-173a-a65559a34eca"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Full, cross-tenant administrative access to the entire platform.", true, "Super Administrator" },
                    { new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Manages privacy compliance activities: assessments, findings, evidence.", true, "Privacy Officer" },
                    { new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Departmental visibility into assessments, findings, and evidence.", true, "Department Owner" },
                    { new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Oversees compliance posture: approves assessments, closes findings.", true, "Compliance Officer" },
                    { new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Read-only access across the platform, plus the audit trail.", true, "Auditor" },
                    { new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Executive read-only visibility into compliance posture and reports.", true, "Management" },
                    { new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Manages security controls, risk, and incident-adjacent findings.", true, "Security Officer" },
                    { new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Full administrative access within one organisation.", true, "Organisation Administrator" },
                    { new Guid("b98e94e4-9a76-5548-996c-e66e1750e203"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Manages user accounts and technical controls.", true, "IT Administrator" },
                    { new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Minimal read-only access.", true, "Read Only" }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { new Guid("025496a9-f552-36a2-4439-70e5d7732dd8"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("1aee2fe5-144e-d283-2f85-761b8641974e"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("1dd266e6-943a-c8e6-439e-c591fadbfe47"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("270b3031-1aa9-3b41-9a31-af214c8a2ce8"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("52e1133d-e4e1-d3f4-af93-6ec8397d590f"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("533af64f-df74-7a52-8af7-e511db0e93d4"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("56f53b5e-cc9d-a5ab-a530-156b8e41338b"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("5b1cabb3-30a0-dc0c-bc2d-c160cf86c6ee"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("5cae153a-0854-ee73-1a78-b7053d97b7b3"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("5e1669e0-3f9e-22eb-841f-68cac453d67e"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("67227c72-d6b2-f692-36f8-77d2928cc300"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("6caa7dbb-db5e-7f31-7c2a-82eecc185418"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("8f668761-f357-5e37-cc7f-65980d2c4adb"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("988207d4-1001-ac73-1c9e-4ad2aee51034"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("9a82dbe9-a3ad-f54c-6b31-862958cd736e"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("a5acab85-c910-8b1c-d3b0-b2b8ccbcd1fb"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("a8027187-eae2-da2c-06f5-b78ea7161fc2"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("b34c0975-ac31-2467-c654-0138fd16d87c"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("bf83dd9d-0066-7e7b-ebcd-97d308590c37"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("c4c4da74-fa5e-e2e6-cf26-1e2480932e79"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("cc9f6165-e27e-7892-b1ac-ea069cfa364d"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("cdaa5381-1bf1-cdce-f39c-8e77089e77b7"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("e9d6c2e4-4c7a-9623-4c5d-ea01f69902da"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("ee6179a3-5935-9799-56b8-84d64c94b72f"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("f1aed973-70b5-2e78-4153-0f4a6f1be34f"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("f73a3e44-9927-3d6a-4988-aa89192bc7a4"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("fbdadc99-1531-a501-16d0-21e226fa2805"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("025496a9-f552-36a2-4439-70e5d7732dd8"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("533af64f-df74-7a52-8af7-e511db0e93d4"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("5b1cabb3-30a0-dc0c-bc2d-c160cf86c6ee"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("5cae153a-0854-ee73-1a78-b7053d97b7b3"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("67227c72-d6b2-f692-36f8-77d2928cc300"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("6caa7dbb-db5e-7f31-7c2a-82eecc185418"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("8f668761-f357-5e37-cc7f-65980d2c4adb"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("a5acab85-c910-8b1c-d3b0-b2b8ccbcd1fb"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("a8027187-eae2-da2c-06f5-b78ea7161fc2"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("b34c0975-ac31-2467-c654-0138fd16d87c"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("bf83dd9d-0066-7e7b-ebcd-97d308590c37"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("cc9f6165-e27e-7892-b1ac-ea069cfa364d"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("cdaa5381-1bf1-cdce-f39c-8e77089e77b7"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("ee6179a3-5935-9799-56b8-84d64c94b72f"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("f1aed973-70b5-2e78-4153-0f4a6f1be34f"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("533af64f-df74-7a52-8af7-e511db0e93d4"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") },
                    { new Guid("5b1cabb3-30a0-dc0c-bc2d-c160cf86c6ee"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") },
                    { new Guid("8f668761-f357-5e37-cc7f-65980d2c4adb"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") },
                    { new Guid("a5acab85-c910-8b1c-d3b0-b2b8ccbcd1fb"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") },
                    { new Guid("b34c0975-ac31-2467-c654-0138fd16d87c"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") },
                    { new Guid("cc9f6165-e27e-7892-b1ac-ea069cfa364d"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") },
                    { new Guid("ee6179a3-5935-9799-56b8-84d64c94b72f"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") },
                    { new Guid("f1aed973-70b5-2e78-4153-0f4a6f1be34f"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") },
                    { new Guid("025496a9-f552-36a2-4439-70e5d7732dd8"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("1dd266e6-943a-c8e6-439e-c591fadbfe47"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("533af64f-df74-7a52-8af7-e511db0e93d4"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("5cae153a-0854-ee73-1a78-b7053d97b7b3"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("5e1669e0-3f9e-22eb-841f-68cac453d67e"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("67227c72-d6b2-f692-36f8-77d2928cc300"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("8f668761-f357-5e37-cc7f-65980d2c4adb"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("988207d4-1001-ac73-1c9e-4ad2aee51034"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("a5acab85-c910-8b1c-d3b0-b2b8ccbcd1fb"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("a8027187-eae2-da2c-06f5-b78ea7161fc2"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("b34c0975-ac31-2467-c654-0138fd16d87c"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("bf83dd9d-0066-7e7b-ebcd-97d308590c37"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("c4c4da74-fa5e-e2e6-cf26-1e2480932e79"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("cc9f6165-e27e-7892-b1ac-ea069cfa364d"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("cdaa5381-1bf1-cdce-f39c-8e77089e77b7"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("ee6179a3-5935-9799-56b8-84d64c94b72f"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("f1aed973-70b5-2e78-4153-0f4a6f1be34f"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("f73a3e44-9927-3d6a-4988-aa89192bc7a4"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("fbdadc99-1531-a501-16d0-21e226fa2805"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("533af64f-df74-7a52-8af7-e511db0e93d4"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("5cae153a-0854-ee73-1a78-b7053d97b7b3"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("67227c72-d6b2-f692-36f8-77d2928cc300"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("8f668761-f357-5e37-cc7f-65980d2c4adb"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("a5acab85-c910-8b1c-d3b0-b2b8ccbcd1fb"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("a8027187-eae2-da2c-06f5-b78ea7161fc2"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("b34c0975-ac31-2467-c654-0138fd16d87c"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("cc9f6165-e27e-7892-b1ac-ea069cfa364d"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("cdaa5381-1bf1-cdce-f39c-8e77089e77b7"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("ee6179a3-5935-9799-56b8-84d64c94b72f"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("f1aed973-70b5-2e78-4153-0f4a6f1be34f"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("025496a9-f552-36a2-4439-70e5d7732dd8"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") },
                    { new Guid("533af64f-df74-7a52-8af7-e511db0e93d4"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") },
                    { new Guid("67227c72-d6b2-f692-36f8-77d2928cc300"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") },
                    { new Guid("a5acab85-c910-8b1c-d3b0-b2b8ccbcd1fb"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") },
                    { new Guid("cdaa5381-1bf1-cdce-f39c-8e77089e77b7"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") },
                    { new Guid("ee6179a3-5935-9799-56b8-84d64c94b72f"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") },
                    { new Guid("f1aed973-70b5-2e78-4153-0f4a6f1be34f"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") },
                    { new Guid("1dd266e6-943a-c8e6-439e-c591fadbfe47"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("533af64f-df74-7a52-8af7-e511db0e93d4"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("56f53b5e-cc9d-a5ab-a530-156b8e41338b"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("5cae153a-0854-ee73-1a78-b7053d97b7b3"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("5e1669e0-3f9e-22eb-841f-68cac453d67e"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("67227c72-d6b2-f692-36f8-77d2928cc300"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("8f668761-f357-5e37-cc7f-65980d2c4adb"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("988207d4-1001-ac73-1c9e-4ad2aee51034"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("a5acab85-c910-8b1c-d3b0-b2b8ccbcd1fb"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("a8027187-eae2-da2c-06f5-b78ea7161fc2"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("b34c0975-ac31-2467-c654-0138fd16d87c"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("c4c4da74-fa5e-e2e6-cf26-1e2480932e79"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("cc9f6165-e27e-7892-b1ac-ea069cfa364d"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("cdaa5381-1bf1-cdce-f39c-8e77089e77b7"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("f1aed973-70b5-2e78-4153-0f4a6f1be34f"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("f73a3e44-9927-3d6a-4988-aa89192bc7a4"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("025496a9-f552-36a2-4439-70e5d7732dd8"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("1aee2fe5-144e-d283-2f85-761b8641974e"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("270b3031-1aa9-3b41-9a31-af214c8a2ce8"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("52e1133d-e4e1-d3f4-af93-6ec8397d590f"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("533af64f-df74-7a52-8af7-e511db0e93d4"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("5cae153a-0854-ee73-1a78-b7053d97b7b3"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("67227c72-d6b2-f692-36f8-77d2928cc300"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("8f668761-f357-5e37-cc7f-65980d2c4adb"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("9a82dbe9-a3ad-f54c-6b31-862958cd736e"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("a5acab85-c910-8b1c-d3b0-b2b8ccbcd1fb"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("a8027187-eae2-da2c-06f5-b78ea7161fc2"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("b34c0975-ac31-2467-c654-0138fd16d87c"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("cc9f6165-e27e-7892-b1ac-ea069cfa364d"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("cdaa5381-1bf1-cdce-f39c-8e77089e77b7"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("e9d6c2e4-4c7a-9623-4c5d-ea01f69902da"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("ee6179a3-5935-9799-56b8-84d64c94b72f"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("f1aed973-70b5-2e78-4153-0f4a6f1be34f"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("1aee2fe5-144e-d283-2f85-761b8641974e"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("270b3031-1aa9-3b41-9a31-af214c8a2ce8"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("52e1133d-e4e1-d3f4-af93-6ec8397d590f"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("5b1cabb3-30a0-dc0c-bc2d-c160cf86c6ee"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("5cae153a-0854-ee73-1a78-b7053d97b7b3"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("8f668761-f357-5e37-cc7f-65980d2c4adb"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("a5acab85-c910-8b1c-d3b0-b2b8ccbcd1fb"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("a8027187-eae2-da2c-06f5-b78ea7161fc2"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("cc9f6165-e27e-7892-b1ac-ea069cfa364d"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("cdaa5381-1bf1-cdce-f39c-8e77089e77b7"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("533af64f-df74-7a52-8af7-e511db0e93d4"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("5cae153a-0854-ee73-1a78-b7053d97b7b3"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("67227c72-d6b2-f692-36f8-77d2928cc300"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("8f668761-f357-5e37-cc7f-65980d2c4adb"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("a5acab85-c910-8b1c-d3b0-b2b8ccbcd1fb"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("a8027187-eae2-da2c-06f5-b78ea7161fc2"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("b34c0975-ac31-2467-c654-0138fd16d87c"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("cc9f6165-e27e-7892-b1ac-ea069cfa364d"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("cdaa5381-1bf1-cdce-f39c-8e77089e77b7"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("ee6179a3-5935-9799-56b8-84d64c94b72f"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("f1aed973-70b5-2e78-4153-0f4a6f1be34f"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_correlation_id",
                table: "audit_logs",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity_type_entity_id",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_organisation_id_created_at",
                table: "audit_logs",
                columns: new[] { "organisation_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_login_history_organisation_id_created_at",
                table: "login_history",
                columns: new[] { "organisation_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_login_history_user_id",
                table: "login_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_tokens_token_hash",
                table: "password_reset_tokens",
                column: "token_hash");

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_tokens_user_id",
                table: "password_reset_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_key",
                table: "permissions",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id_role_id",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true,
                filter: "organisation_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id_role_id_organisation_id",
                table: "user_roles",
                columns: new[] { "user_id", "role_id", "organisation_id" },
                unique: true,
                filter: "organisation_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_normalized_email",
                table: "users",
                column: "normalized_email",
                unique: true,
                filter: "organisation_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_organisation_id",
                table: "users",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_organisation_id_normalized_email",
                table: "users",
                columns: new[] { "organisation_id", "normalized_email" },
                unique: true,
                filter: "organisation_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "login_history");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "organisations");
        }
    }
}
