using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DPDP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrganisationManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "organisations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dpo_email",
                table: "organisations",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dpo_name",
                table: "organisations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dpo_phone",
                table: "organisations",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "industry",
                table: "organisations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "primary_contact_email",
                table: "organisations",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "primary_contact_name",
                table: "organisations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "primary_contact_phone",
                table: "organisations",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "privacy_contact_email",
                table: "organisations",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "privacy_contact_name",
                table: "organisations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "privacy_contact_phone",
                table: "organisations",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "size",
                table: "organisations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "website",
                table: "organisations",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "business_units",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    head_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    head_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    head_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("pk_business_units", x => x.id);
                    table.ForeignKey(
                        name: "fk_business_units_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organisation_locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address_line1 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    address_line2 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_organisation_locations", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_locations_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    head_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    head_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    head_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("pk_departments", x => x.id);
                    table.ForeignKey(
                        name: "fk_departments_business_units_business_unit_id",
                        column: x => x.business_unit_id,
                        principalTable: "business_units",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "created_at", "description", "key", "module" },
                values: new object[,]
                {
                    { new Guid("64078104-60a7-68ed-ff34-bfe0324ecc1c"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Create business units.", "businessunits.create", "Organisations" },
                    { new Guid("7f4d3246-d310-7418-d40c-df0f8b41395d"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Edit departments.", "departments.update", "Organisations" },
                    { new Guid("8c855f7c-a7e4-69e6-fe79-99fbc43dae5a"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Edit business units.", "businessunits.update", "Organisations" },
                    { new Guid("96061602-2b0b-7868-e361-c7abd28032b1"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Delete departments.", "departments.delete", "Organisations" },
                    { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View business units.", "businessunits.read", "Organisations" },
                    { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View departments.", "departments.read", "Organisations" },
                    { new Guid("d3e01c55-922c-9458-ba75-a48c134e193f"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Delete business units.", "businessunits.delete", "Organisations" },
                    { new Guid("e02a0a53-323d-86e3-d6ae-695506dcc1dc"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Create departments.", "departments.create", "Organisations" }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { new Guid("64078104-60a7-68ed-ff34-bfe0324ecc1c"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("7f4d3246-d310-7418-d40c-df0f8b41395d"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("8c855f7c-a7e4-69e6-fe79-99fbc43dae5a"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("96061602-2b0b-7868-e361-c7abd28032b1"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("d3e01c55-922c-9458-ba75-a48c134e193f"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("e02a0a53-323d-86e3-d6ae-695506dcc1dc"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") },
                    { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") },
                    { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") },
                    { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") },
                    { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("64078104-60a7-68ed-ff34-bfe0324ecc1c"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("7f4d3246-d310-7418-d40c-df0f8b41395d"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("8c855f7c-a7e4-69e6-fe79-99fbc43dae5a"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("96061602-2b0b-7868-e361-c7abd28032b1"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("d3e01c55-922c-9458-ba75-a48c134e193f"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("e02a0a53-323d-86e3-d6ae-695506dcc1dc"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_business_units_organisation_id",
                table: "business_units",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_business_units_organisation_id_name",
                table: "business_units",
                columns: new[] { "organisation_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_departments_business_unit_id",
                table: "departments",
                column: "business_unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_departments_business_unit_id_name",
                table: "departments",
                columns: new[] { "business_unit_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_departments_organisation_id",
                table: "departments",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_locations_organisation_id",
                table: "organisation_locations",
                column: "organisation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "organisation_locations");

            migrationBuilder.DropTable(
                name: "business_units");

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("64078104-60a7-68ed-ff34-bfe0324ecc1c"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("7f4d3246-d310-7418-d40c-df0f8b41395d"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("8c855f7c-a7e4-69e6-fe79-99fbc43dae5a"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("96061602-2b0b-7868-e361-c7abd28032b1"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("d3e01c55-922c-9458-ba75-a48c134e193f"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e02a0a53-323d-86e3-d6ae-695506dcc1dc"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("64078104-60a7-68ed-ff34-bfe0324ecc1c"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("7f4d3246-d310-7418-d40c-df0f8b41395d"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("8c855f7c-a7e4-69e6-fe79-99fbc43dae5a"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("96061602-2b0b-7868-e361-c7abd28032b1"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("d3e01c55-922c-9458-ba75-a48c134e193f"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e02a0a53-323d-86e3-d6ae-695506dcc1dc"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") });

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("64078104-60a7-68ed-ff34-bfe0324ecc1c"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("7f4d3246-d310-7418-d40c-df0f8b41395d"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("8c855f7c-a7e4-69e6-fe79-99fbc43dae5a"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("96061602-2b0b-7868-e361-c7abd28032b1"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("b2cb4244-a59e-5d10-d47a-c0fe3107a808"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("d1c9a18b-7424-d947-8ab9-10451a36f46c"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("d3e01c55-922c-9458-ba75-a48c134e193f"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("e02a0a53-323d-86e3-d6ae-695506dcc1dc"));

            migrationBuilder.DropColumn(
                name: "country",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "dpo_email",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "dpo_name",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "dpo_phone",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "industry",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "primary_contact_email",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "primary_contact_name",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "primary_contact_phone",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "privacy_contact_email",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "privacy_contact_name",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "privacy_contact_phone",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "size",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "website",
                table: "organisations");
        }
    }
}
