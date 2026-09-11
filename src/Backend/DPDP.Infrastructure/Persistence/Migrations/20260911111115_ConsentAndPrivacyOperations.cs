using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DPDP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConsentAndPrivacyOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "consent_purposes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    data_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_consent_purposes", x => x.id);
                    table.ForeignKey(
                        name: "fk_consent_purposes_data_categories_data_category_id",
                        column: x => x.data_category_id,
                        principalTable: "data_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "data_principals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_reference_id = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    reference_category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_data_principals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "privacy_notices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    purpose = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    published_date = table.Column<DateOnly>(type: "date", nullable: true),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_privacy_notices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sla_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    request_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    response_due_days = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_sla_policies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "consent_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_principal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consent_purpose_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notice_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    granted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    channel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    withdrawn_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    source_system = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    external_reference_id = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consent_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_consent_records_consent_purposes_consent_purpose_id",
                        column: x => x.consent_purpose_id,
                        principalTable: "consent_purposes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_consent_records_data_principals_data_principal_id",
                        column: x => x.data_principal_id,
                        principalTable: "data_principals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_consent_records_privacy_notices_notice_version_id",
                        column: x => x.notice_version_id,
                        principalTable: "privacy_notices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "privacy_notice_data_categories",
                columns: table => new
                {
                    data_categories_id = table.Column<Guid>(type: "uuid", nullable: false),
                    privacy_notice_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_privacy_notice_data_categories", x => new { x.data_categories_id, x.privacy_notice_id });
                    table.ForeignKey(
                        name: "fk_privacy_notice_data_categories_data_categories_data_categor",
                        column: x => x.data_categories_id,
                        principalTable: "data_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_privacy_notice_data_categories_privacy_notices_privacy_noti",
                        column: x => x.privacy_notice_id,
                        principalTable: "privacy_notices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_principal_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    request_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    requester_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    requester_contact_email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    requester_contact_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    external_reference_id = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    data_principal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    related_consent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    sla_policy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    identity_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    identity_verified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_to_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    resolution_notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    rejected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_data_principal_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_principal_requests_consent_records_related_consent_id",
                        column: x => x.related_consent_id,
                        principalTable: "consent_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_principal_requests_data_principals_data_principal_id",
                        column: x => x.data_principal_id,
                        principalTable: "data_principals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_principal_requests_sla_policies_sla_policy_id",
                        column: x => x.sla_policy_id,
                        principalTable: "sla_policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_principal_requests_users_assigned_to_user_id",
                        column: x => x.assigned_to_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "created_at", "description", "key", "module" },
                values: new object[,]
                {
                    { new Guid("0396be2a-4dff-da26-0400-2405e7a6dc08"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Create and edit consent purposes.", "consentpurposes.manage", "ConsentPrivacy" },
                    { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View consent purposes.", "consentpurposes.read", "ConsentPrivacy" },
                    { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View data principal requests and grievances.", "datarequests.read", "ConsentPrivacy" },
                    { new Guid("6676d15e-ae1c-3399-f092-35b8f14a6801"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Approve a privacy notice before publication.", "privacynotices.approve", "ConsentPrivacy" },
                    { new Guid("7697a523-3b3d-ccb6-4646-9de326908690"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Capture, withdraw, and revoke consent records.", "consent.manage", "ConsentPrivacy" },
                    { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View privacy notices.", "privacynotices.read", "ConsentPrivacy" },
                    { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View consent records.", "consent.read", "ConsentPrivacy" },
                    { new Guid("e1bec7bf-77ff-7540-c5b9-73080bfb983b"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Create and edit data principal references.", "dataprincipals.manage", "ConsentPrivacy" },
                    { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View data principal references.", "dataprincipals.read", "ConsentPrivacy" },
                    { new Guid("e82379ed-429e-99dd-3609-bfa306a86614"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Manage data principal requests and grievances, including SLA policies.", "datarequests.manage", "ConsentPrivacy" },
                    { new Guid("ecf41ed7-3cb1-6ea4-c6da-981d8d7f9e61"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Create, edit, publish, and archive privacy notices.", "privacynotices.manage", "ConsentPrivacy" }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { new Guid("0396be2a-4dff-da26-0400-2405e7a6dc08"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("6676d15e-ae1c-3399-f092-35b8f14a6801"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("7697a523-3b3d-ccb6-4646-9de326908690"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("e1bec7bf-77ff-7540-c5b9-73080bfb983b"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("e82379ed-429e-99dd-3609-bfa306a86614"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("ecf41ed7-3cb1-6ea4-c6da-981d8d7f9e61"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("0396be2a-4dff-da26-0400-2405e7a6dc08"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("7697a523-3b3d-ccb6-4646-9de326908690"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("e1bec7bf-77ff-7540-c5b9-73080bfb983b"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("e82379ed-429e-99dd-3609-bfa306a86614"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("ecf41ed7-3cb1-6ea4-c6da-981d8d7f9e61"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("6676d15e-ae1c-3399-f092-35b8f14a6801"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") },
                    { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_consent_purposes_data_category_id",
                table: "consent_purposes",
                column: "data_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_consent_purposes_organisation_id",
                table: "consent_purposes",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_consent_records_consent_purpose_id",
                table: "consent_records",
                column: "consent_purpose_id");

            migrationBuilder.CreateIndex(
                name: "ix_consent_records_data_principal_id",
                table: "consent_records",
                column: "data_principal_id");

            migrationBuilder.CreateIndex(
                name: "ix_consent_records_expires_at",
                table: "consent_records",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_consent_records_notice_version_id",
                table: "consent_records",
                column: "notice_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_consent_records_organisation_id",
                table: "consent_records",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_consent_records_organisation_id_sequence_number",
                table: "consent_records",
                columns: new[] { "organisation_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_consent_records_organisation_id_status",
                table: "consent_records",
                columns: new[] { "organisation_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_data_principal_requests_assigned_to_user_id",
                table: "data_principal_requests",
                column: "assigned_to_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_principal_requests_data_principal_id",
                table: "data_principal_requests",
                column: "data_principal_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_principal_requests_due_at",
                table: "data_principal_requests",
                column: "due_at");

            migrationBuilder.CreateIndex(
                name: "ix_data_principal_requests_organisation_id",
                table: "data_principal_requests",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_principal_requests_organisation_id_request_type",
                table: "data_principal_requests",
                columns: new[] { "organisation_id", "request_type" });

            migrationBuilder.CreateIndex(
                name: "ix_data_principal_requests_organisation_id_sequence_number",
                table: "data_principal_requests",
                columns: new[] { "organisation_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_data_principal_requests_organisation_id_status",
                table: "data_principal_requests",
                columns: new[] { "organisation_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_data_principal_requests_related_consent_id",
                table: "data_principal_requests",
                column: "related_consent_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_principal_requests_sla_policy_id",
                table: "data_principal_requests",
                column: "sla_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_principals_organisation_id",
                table: "data_principals",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_principals_organisation_id_external_reference_id",
                table: "data_principals",
                columns: new[] { "organisation_id", "external_reference_id" });

            migrationBuilder.CreateIndex(
                name: "ix_privacy_notice_data_categories_privacy_notice_id",
                table: "privacy_notice_data_categories",
                column: "privacy_notice_id");

            migrationBuilder.CreateIndex(
                name: "ix_privacy_notices_organisation_id_code_version",
                table: "privacy_notices",
                columns: new[] { "organisation_id", "code", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_privacy_notices_organisation_id_sequence_number",
                table: "privacy_notices",
                columns: new[] { "organisation_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_privacy_notices_organisation_id_status",
                table: "privacy_notices",
                columns: new[] { "organisation_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_sla_policies_organisation_id",
                table: "sla_policies",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_sla_policies_organisation_id_request_type",
                table: "sla_policies",
                columns: new[] { "organisation_id", "request_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_principal_requests");

            migrationBuilder.DropTable(
                name: "privacy_notice_data_categories");

            migrationBuilder.DropTable(
                name: "consent_records");

            migrationBuilder.DropTable(
                name: "sla_policies");

            migrationBuilder.DropTable(
                name: "consent_purposes");

            migrationBuilder.DropTable(
                name: "data_principals");

            migrationBuilder.DropTable(
                name: "privacy_notices");

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("0396be2a-4dff-da26-0400-2405e7a6dc08"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6676d15e-ae1c-3399-f092-35b8f14a6801"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("7697a523-3b3d-ccb6-4646-9de326908690"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e1bec7bf-77ff-7540-c5b9-73080bfb983b"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e82379ed-429e-99dd-3609-bfa306a86614"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("ecf41ed7-3cb1-6ea4-c6da-981d8d7f9e61"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("0396be2a-4dff-da26-0400-2405e7a6dc08"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("7697a523-3b3d-ccb6-4646-9de326908690"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e1bec7bf-77ff-7540-c5b9-73080bfb983b"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e82379ed-429e-99dd-3609-bfa306a86614"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("ecf41ed7-3cb1-6ea4-c6da-981d8d7f9e61"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6676d15e-ae1c-3399-f092-35b8f14a6801"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c0259a99-9211-44e1-3337-658459b08964"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") });

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("0396be2a-4dff-da26-0400-2405e7a6dc08"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("216d12ce-7722-79b2-d7f4-324a78fbe540"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("2ce27238-bbda-5e5e-e7d6-0516838f933f"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("6676d15e-ae1c-3399-f092-35b8f14a6801"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("7697a523-3b3d-ccb6-4646-9de326908690"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("c0259a99-9211-44e1-3337-658459b08964"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("c71ec89d-74f1-1882-f5c4-f01ae9b36eb3"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("e1bec7bf-77ff-7540-c5b9-73080bfb983b"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("e7857e70-0dbc-9e89-f76a-08724890df74"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("e82379ed-429e-99dd-3609-bfa306a86614"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("ecf41ed7-3cb1-6ea4-c6da-981d8d7f9e61"));
        }
    }
}
