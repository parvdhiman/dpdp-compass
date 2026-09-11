using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DPDP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EvidenceManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "evidence_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    evidence_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    assessment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    control_id = table.Column<Guid>(type: "uuid", nullable: true),
                    finding_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vendor_reference = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    processing_activity_reference = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewer_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expiry_date = table.Column<DateOnly>(type: "date", nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    rejected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    rejected_by = table.Column<Guid>(type: "uuid", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    archived_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    archived_by = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("pk_evidence_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_evidence_items_assessments_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "assessments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_evidence_items_controls_control_id",
                        column: x => x.control_id,
                        principalTable: "controls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_evidence_items_findings_finding_id",
                        column: x => x.finding_id,
                        principalTable: "findings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_evidence_items_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_evidence_items_users_reviewer_user_id",
                        column: x => x.reviewer_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "evidence_review_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evidence_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evidence_version_number = table.Column<int>(type: "integer", nullable: false),
                    reviewer_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    comments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evidence_review_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_evidence_review_records_evidence_items_evidence_item_id",
                        column: x => x.evidence_item_id,
                        principalTable: "evidence_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_evidence_review_records_users_reviewer_user_id",
                        column: x => x.reviewer_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "evidence_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evidence_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    original_file_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    content_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    checksum_sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    external_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    malware_scan_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    malware_scan_details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    uploaded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evidence_versions", x => x.id);
                    table.ForeignKey(
                        name: "fk_evidence_versions_evidence_items_evidence_item_id",
                        column: x => x.evidence_item_id,
                        principalTable: "evidence_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_evidence_versions_users_uploaded_by_user_id",
                        column: x => x.uploaded_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_evidence_items_assessment_id",
                table: "evidence_items",
                column: "assessment_id");

            migrationBuilder.CreateIndex(
                name: "ix_evidence_items_control_id",
                table: "evidence_items",
                column: "control_id");

            migrationBuilder.CreateIndex(
                name: "ix_evidence_items_expiry_date",
                table: "evidence_items",
                column: "expiry_date");

            migrationBuilder.CreateIndex(
                name: "ix_evidence_items_finding_id",
                table: "evidence_items",
                column: "finding_id");

            migrationBuilder.CreateIndex(
                name: "ix_evidence_items_organisation_id",
                table: "evidence_items",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_evidence_items_organisation_id_evidence_type",
                table: "evidence_items",
                columns: new[] { "organisation_id", "evidence_type" });

            migrationBuilder.CreateIndex(
                name: "ix_evidence_items_organisation_id_sequence_number",
                table: "evidence_items",
                columns: new[] { "organisation_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_evidence_items_organisation_id_status",
                table: "evidence_items",
                columns: new[] { "organisation_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_evidence_items_owner_user_id",
                table: "evidence_items",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_evidence_items_reviewer_user_id",
                table: "evidence_items",
                column: "reviewer_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_evidence_review_records_evidence_item_id",
                table: "evidence_review_records",
                column: "evidence_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_evidence_review_records_organisation_id",
                table: "evidence_review_records",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_evidence_review_records_reviewer_user_id",
                table: "evidence_review_records",
                column: "reviewer_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_evidence_versions_evidence_item_id_version_number",
                table: "evidence_versions",
                columns: new[] { "evidence_item_id", "version_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_evidence_versions_organisation_id",
                table: "evidence_versions",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_evidence_versions_uploaded_by_user_id",
                table: "evidence_versions",
                column: "uploaded_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evidence_review_records");

            migrationBuilder.DropTable(
                name: "evidence_versions");

            migrationBuilder.DropTable(
                name: "evidence_items");
        }
    }
}
