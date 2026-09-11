using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DPDP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DataDiscovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_sources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    source_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    host = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    port = table.Column<int>(type: "integer", nullable: true),
                    database_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    username = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    encrypted_secret = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    schema_filter = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    root_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_tested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_test_succeeded = table.Column<bool>(type: "boolean", nullable: true),
                    last_test_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("pk_data_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_assets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    database_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    schema_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    asset_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    asset_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    file_path = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    estimated_row_count = table.Column<long>(type: "bigint", nullable: true),
                    indexes_json = table.Column<string>(type: "jsonb", nullable: true),
                    last_discovered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_discovery_job_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("pk_data_assets", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_assets_data_sources_data_source_id",
                        column: x => x.data_source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "discovery_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    triggered_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    cancellation_requested = table.Column<bool>(type: "boolean", nullable: false),
                    assets_discovered_count = table.Column<int>(type: "integer", nullable: false),
                    elements_discovered_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discovery_jobs", x => x.id);
                    table.ForeignKey(
                        name: "fk_discovery_jobs_data_sources_data_source_id",
                        column: x => x.data_source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_discovery_jobs_users_triggered_by_user_id",
                        column: x => x.triggered_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "data_elements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    column_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    data_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_nullable = table.Column<bool>(type: "boolean", nullable: false),
                    ordinal_position = table.Column<int>(type: "integer", nullable: false),
                    sample_masked_value = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    classification_category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    classification_confidence = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    classification_source = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    is_human_corrected = table.Column<bool>(type: "boolean", nullable: false),
                    corrected_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    corrected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_discovered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_elements", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_elements_data_assets_data_asset_id",
                        column: x => x.data_asset_id,
                        principalTable: "data_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_data_elements_users_corrected_by_user_id",
                        column: x => x.corrected_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "discovery_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discovery_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    row_count_at_scan = table.Column<long>(type: "bigint", nullable: true),
                    columns_discovered = table.Column<int>(type: "integer", nullable: false),
                    indexes_json = table.Column<string>(type: "jsonb", nullable: true),
                    scanned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discovery_results", x => x.id);
                    table.ForeignKey(
                        name: "fk_discovery_results_data_assets_data_asset_id",
                        column: x => x.data_asset_id,
                        principalTable: "data_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_discovery_results_discovery_jobs_discovery_job_id",
                        column: x => x.discovery_job_id,
                        principalTable: "discovery_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "created_at", "description", "key", "module" },
                values: new object[,]
                {
                    { new Guid("4897e39e-ba78-e7ab-4ae4-6331de0e0163"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Correct a data element's classification.", "classification.review", "DataDiscovery" },
                    { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View discovered data assets and elements.", "dataassets.read", "DataDiscovery" },
                    { new Guid("7f26d49a-21a7-3b4c-0b79-a08cd6f67dc1"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Register, edit, test, and remove data sources.", "datasources.manage", "DataDiscovery" },
                    { new Guid("cd2b52c9-a59d-7701-c260-a22237af40f8"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Start and cancel discovery jobs.", "discoveryjobs.manage", "DataDiscovery" },
                    { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View registered data sources.", "datasources.read", "DataDiscovery" },
                    { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View discovery jobs.", "discoveryjobs.read", "DataDiscovery" }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { new Guid("4897e39e-ba78-e7ab-4ae4-6331de0e0163"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("7f26d49a-21a7-3b4c-0b79-a08cd6f67dc1"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("cd2b52c9-a59d-7701-c260-a22237af40f8"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("4897e39e-ba78-e7ab-4ae4-6331de0e0163"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("7f26d49a-21a7-3b4c-0b79-a08cd6f67dc1"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("cd2b52c9-a59d-7701-c260-a22237af40f8"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") },
                    { new Guid("4897e39e-ba78-e7ab-4ae4-6331de0e0163"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") },
                    { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("7f26d49a-21a7-3b4c-0b79-a08cd6f67dc1"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("cd2b52c9-a59d-7701-c260-a22237af40f8"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("7f26d49a-21a7-3b4c-0b79-a08cd6f67dc1"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("cd2b52c9-a59d-7701-c260-a22237af40f8"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_data_assets_data_source_id",
                table: "data_assets",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_assets_data_source_id_schema_name_asset_name",
                table: "data_assets",
                columns: new[] { "data_source_id", "schema_name", "asset_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_data_assets_organisation_id",
                table: "data_assets",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_elements_corrected_by_user_id",
                table: "data_elements",
                column: "corrected_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_elements_data_asset_id",
                table: "data_elements",
                column: "data_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_elements_data_asset_id_column_name",
                table: "data_elements",
                columns: new[] { "data_asset_id", "column_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_data_elements_organisation_id",
                table: "data_elements",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_elements_organisation_id_classification_category",
                table: "data_elements",
                columns: new[] { "organisation_id", "classification_category" });

            migrationBuilder.CreateIndex(
                name: "ix_data_sources_organisation_id",
                table: "data_sources",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_sources_organisation_id_source_type",
                table: "data_sources",
                columns: new[] { "organisation_id", "source_type" });

            migrationBuilder.CreateIndex(
                name: "ix_discovery_jobs_data_source_id",
                table: "discovery_jobs",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_discovery_jobs_organisation_id",
                table: "discovery_jobs",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_discovery_jobs_organisation_id_status",
                table: "discovery_jobs",
                columns: new[] { "organisation_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_discovery_jobs_triggered_by_user_id",
                table: "discovery_jobs",
                column: "triggered_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_discovery_results_data_asset_id",
                table: "discovery_results",
                column: "data_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_discovery_results_discovery_job_id",
                table: "discovery_results",
                column: "discovery_job_id");

            migrationBuilder.CreateIndex(
                name: "ix_discovery_results_organisation_id",
                table: "discovery_results",
                column: "organisation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_elements");

            migrationBuilder.DropTable(
                name: "discovery_results");

            migrationBuilder.DropTable(
                name: "data_assets");

            migrationBuilder.DropTable(
                name: "discovery_jobs");

            migrationBuilder.DropTable(
                name: "data_sources");

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("4897e39e-ba78-e7ab-4ae4-6331de0e0163"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("7f26d49a-21a7-3b4c-0b79-a08cd6f67dc1"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("cd2b52c9-a59d-7701-c260-a22237af40f8"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("4897e39e-ba78-e7ab-4ae4-6331de0e0163"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("7f26d49a-21a7-3b4c-0b79-a08cd6f67dc1"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("cd2b52c9-a59d-7701-c260-a22237af40f8"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("4897e39e-ba78-e7ab-4ae4-6331de0e0163"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("7f26d49a-21a7-3b4c-0b79-a08cd6f67dc1"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("cd2b52c9-a59d-7701-c260-a22237af40f8"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("7f26d49a-21a7-3b4c-0b79-a08cd6f67dc1"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("cd2b52c9-a59d-7701-c260-a22237af40f8"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("6577da9b-2f45-5068-7897-b71927537372"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") });

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("4897e39e-ba78-e7ab-4ae4-6331de0e0163"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("6577da9b-2f45-5068-7897-b71927537372"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("7f26d49a-21a7-3b4c-0b79-a08cd6f67dc1"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("cd2b52c9-a59d-7701-c260-a22237af40f8"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("e18bbdeb-72dd-5cd3-86da-497a2f4036c0"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("e79a6cf0-979b-7c5e-3030-3002963c4df0"));
        }
    }
}
