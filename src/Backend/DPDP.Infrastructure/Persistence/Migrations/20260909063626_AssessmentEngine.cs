using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DPDP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AssessmentEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assessments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    framework_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    assigned_to_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    submitted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    decided_by = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("pk_assessments", x => x.id);
                    table.ForeignKey(
                        name: "fk_assessments_framework_versions_framework_version_id",
                        column: x => x.framework_version_id,
                        principalTable: "compliance_framework_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assessments_users_assigned_to_user_id",
                        column: x => x.assigned_to_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "assessment_approvals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assessment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    decided_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    comments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assessment_approvals", x => x.id);
                    table.ForeignKey(
                        name: "fk_assessment_approvals_assessments_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "assessments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_assessment_approvals_users_decided_by_user_id",
                        column: x => x.decided_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assessment_controls",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assessment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    control_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assessment_controls", x => x.id);
                    table.ForeignKey(
                        name: "fk_assessment_controls_assessments_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "assessments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_assessment_controls_controls_control_id",
                        column: x => x.control_id,
                        principalTable: "controls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assessment_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assessment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    comments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assessment_reviews", x => x.id);
                    table.ForeignKey(
                        name: "fk_assessment_reviews_assessments_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "assessments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_assessment_reviews_users_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assessment_scopes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assessment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_unit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assessment_scopes", x => x.id);
                    table.ForeignKey(
                        name: "fk_assessment_scopes_assessments_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "assessments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_assessment_scopes_business_units_business_unit_id",
                        column: x => x.business_unit_id,
                        principalTable: "business_units",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assessment_scopes_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assessment_control_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assessment_control_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assessment_control_questions", x => x.id);
                    table.ForeignKey(
                        name: "fk_assessment_control_questions_assessment_controls_assessment",
                        column: x => x.assessment_control_id,
                        principalTable: "assessment_controls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_assessment_control_questions_assessment_questions_question_",
                        column: x => x.question_id,
                        principalTable: "assessment_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assessment_answers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assessment_control_question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    answer_value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    answer_values_json = table.Column<string>(type: "jsonb", nullable: true),
                    comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    evidence_json = table.Column<string>(type: "jsonb", nullable: true),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    review_comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    confidence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    assessed_risk_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    remediation_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assessment_answers", x => x.id);
                    table.ForeignKey(
                        name: "fk_assessment_answers_assessment_control_questions_assessment_",
                        column: x => x.assessment_control_question_id,
                        principalTable: "assessment_control_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_assessment_answers_users_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_assessment_answers_assessment_control_question_id",
                table: "assessment_answers",
                column: "assessment_control_question_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assessment_answers_organisation_id",
                table: "assessment_answers",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_answers_reviewer_id",
                table: "assessment_answers",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_answers_status",
                table: "assessment_answers",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_approvals_assessment_id",
                table: "assessment_approvals",
                column: "assessment_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_approvals_decided_by_user_id",
                table: "assessment_approvals",
                column: "decided_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_approvals_organisation_id",
                table: "assessment_approvals",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_control_questions_assessment_control_id",
                table: "assessment_control_questions",
                column: "assessment_control_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_control_questions_assessment_control_id_question",
                table: "assessment_control_questions",
                columns: new[] { "assessment_control_id", "question_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assessment_control_questions_organisation_id",
                table: "assessment_control_questions",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_control_questions_question_id",
                table: "assessment_control_questions",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_controls_assessment_id",
                table: "assessment_controls",
                column: "assessment_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_controls_assessment_id_control_id",
                table: "assessment_controls",
                columns: new[] { "assessment_id", "control_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assessment_controls_control_id",
                table: "assessment_controls",
                column: "control_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_controls_organisation_id",
                table: "assessment_controls",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_controls_status",
                table: "assessment_controls",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_reviews_assessment_id",
                table: "assessment_reviews",
                column: "assessment_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_reviews_organisation_id",
                table: "assessment_reviews",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_reviews_reviewer_id",
                table: "assessment_reviews",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_scopes_assessment_id",
                table: "assessment_scopes",
                column: "assessment_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_scopes_business_unit_id",
                table: "assessment_scopes",
                column: "business_unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_scopes_department_id",
                table: "assessment_scopes",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessment_scopes_organisation_id",
                table: "assessment_scopes",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessments_assigned_to_user_id",
                table: "assessments",
                column: "assigned_to_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessments_framework_version_id",
                table: "assessments",
                column: "framework_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessments_organisation_id",
                table: "assessments",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_assessments_organisation_id_status",
                table: "assessments",
                columns: new[] { "organisation_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assessment_answers");

            migrationBuilder.DropTable(
                name: "assessment_approvals");

            migrationBuilder.DropTable(
                name: "assessment_reviews");

            migrationBuilder.DropTable(
                name: "assessment_scopes");

            migrationBuilder.DropTable(
                name: "assessment_control_questions");

            migrationBuilder.DropTable(
                name: "assessment_controls");

            migrationBuilder.DropTable(
                name: "assessments");
        }
    }
}
