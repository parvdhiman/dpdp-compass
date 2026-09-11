using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DPDP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FindingsRiskRemediation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "risks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    likelihood = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    impact = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_sensitivity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    exposure = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    calculated_risk_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    calculated_risk_score = table.Column<double>(type: "double precision", nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    treatment_plan = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("pk_risks", x => x.id);
                    table.ForeignKey(
                        name: "fk_risks_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "findings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    assessment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assessment_control_id = table.Column<Guid>(type: "uuid", nullable: true),
                    control_id = table.Column<Guid>(type: "uuid", nullable: true),
                    asset_reference = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    risk_id = table.Column<Guid>(type: "uuid", nullable: true),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    recommendation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("pk_findings", x => x.id);
                    table.ForeignKey(
                        name: "fk_findings_assessment_controls_assessment_control_id",
                        column: x => x.assessment_control_id,
                        principalTable: "assessment_controls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_findings_assessments_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "assessments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_findings_controls_control_id",
                        column: x => x.control_id,
                        principalTable: "controls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_findings_risks_risk_id",
                        column: x => x.risk_id,
                        principalTable: "risks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_findings_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "remediation_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    finding_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    evidence_json = table.Column<string>(type: "jsonb", nullable: true),
                    verified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verification_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_remediation_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_remediation_tasks_findings_finding_id",
                        column: x => x.finding_id,
                        principalTable: "findings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_remediation_tasks_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_remediation_tasks_users_verified_by_user_id",
                        column: x => x.verified_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "remediation_comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    remediation_task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_remediation_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_remediation_comments_remediation_tasks_remediation_task_id",
                        column: x => x.remediation_task_id,
                        principalTable: "remediation_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_remediation_comments_users_author_user_id",
                        column: x => x.author_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { new Guid("56f53b5e-cc9d-a5ab-a530-156b8e41338b"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("6caa7dbb-db5e-7f31-7c2a-82eecc185418"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("6caa7dbb-db5e-7f31-7c2a-82eecc185418"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_findings_assessment_control_id",
                table: "findings",
                column: "assessment_control_id");

            migrationBuilder.CreateIndex(
                name: "ix_findings_assessment_id",
                table: "findings",
                column: "assessment_id");

            migrationBuilder.CreateIndex(
                name: "ix_findings_control_id",
                table: "findings",
                column: "control_id");

            migrationBuilder.CreateIndex(
                name: "ix_findings_organisation_id",
                table: "findings",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_findings_organisation_id_sequence_number",
                table: "findings",
                columns: new[] { "organisation_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_findings_organisation_id_severity",
                table: "findings",
                columns: new[] { "organisation_id", "severity" });

            migrationBuilder.CreateIndex(
                name: "ix_findings_organisation_id_status",
                table: "findings",
                columns: new[] { "organisation_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_findings_owner_user_id",
                table: "findings",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_findings_risk_id",
                table: "findings",
                column: "risk_id");

            migrationBuilder.CreateIndex(
                name: "ix_remediation_comments_author_user_id",
                table: "remediation_comments",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_remediation_comments_organisation_id",
                table: "remediation_comments",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_remediation_comments_remediation_task_id",
                table: "remediation_comments",
                column: "remediation_task_id");

            migrationBuilder.CreateIndex(
                name: "ix_remediation_tasks_due_date",
                table: "remediation_tasks",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "ix_remediation_tasks_finding_id",
                table: "remediation_tasks",
                column: "finding_id");

            migrationBuilder.CreateIndex(
                name: "ix_remediation_tasks_organisation_id",
                table: "remediation_tasks",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_remediation_tasks_organisation_id_status",
                table: "remediation_tasks",
                columns: new[] { "organisation_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_remediation_tasks_owner_user_id",
                table: "remediation_tasks",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_remediation_tasks_verified_by_user_id",
                table: "remediation_tasks",
                column: "verified_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_risks_organisation_id",
                table: "risks",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_risks_organisation_id_calculated_risk_level",
                table: "risks",
                columns: new[] { "organisation_id", "calculated_risk_level" });

            migrationBuilder.CreateIndex(
                name: "ix_risks_organisation_id_sequence_number",
                table: "risks",
                columns: new[] { "organisation_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_risks_organisation_id_status",
                table: "risks",
                columns: new[] { "organisation_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_risks_owner_user_id",
                table: "risks",
                column: "owner_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "remediation_comments");

            migrationBuilder.DropTable(
                name: "remediation_tasks");

            migrationBuilder.DropTable(
                name: "findings");

            migrationBuilder.DropTable(
                name: "risks");

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("56f53b5e-cc9d-a5ab-a530-156b8e41338b"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6caa7dbb-db5e-7f31-7c2a-82eecc185418"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6caa7dbb-db5e-7f31-7c2a-82eecc185418"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });
        }
    }
}
