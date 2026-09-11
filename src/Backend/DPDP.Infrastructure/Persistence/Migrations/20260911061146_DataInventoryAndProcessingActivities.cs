using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DPDP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DataInventoryAndProcessingActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    classification_category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("pk_data_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_collection_sources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    source_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("pk_data_collection_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "it_systems",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    system_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("pk_it_systems", x => x.id);
                    table.ForeignKey(
                        name: "fk_it_systems_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "processors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("pk_processors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recipients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    recipient_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("pk_recipients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "retention_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    retention_period_value = table.Column<int>(type: "integer", nullable: false),
                    retention_period_unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    trigger_event = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("pk_retention_policies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_inventory_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_element_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    discovered_data_element_id = table.Column<Guid>(type: "uuid", nullable: true),
                    classification = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    data_collection_source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    it_system_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    purpose = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    retention_policy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sharing_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    processor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    risk_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("pk_data_inventory_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_inventory_items_data_categories_data_category_id",
                        column: x => x.data_category_id,
                        principalTable: "data_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_data_inventory_items_data_collection_sources_data_collectio",
                        column: x => x.data_collection_source_id,
                        principalTable: "data_collection_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_inventory_items_data_elements_discovered_data_element_",
                        column: x => x.discovered_data_element_id,
                        principalTable: "data_elements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_inventory_items_it_systems_it_system_id",
                        column: x => x.it_system_id,
                        principalTable: "it_systems",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_inventory_items_processors_processor_id",
                        column: x => x.processor_id,
                        principalTable: "processors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_inventory_items_retention_policies_retention_policy_id",
                        column: x => x.retention_policy_id,
                        principalTable: "retention_policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_inventory_items_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "processing_activities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    purpose = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    data_subject_categories_json = table.Column<string>(type: "jsonb", nullable: true),
                    security_controls_json = table.Column<string>(type: "jsonb", nullable: true),
                    retention_policy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    review_date = table.Column<DateOnly>(type: "date", nullable: true),
                    submitted_for_review_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    review_comments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("pk_processing_activities", x => x.id);
                    table.ForeignKey(
                        name: "fk_processing_activities_retention_policies_retention_policy_id",
                        column: x => x.retention_policy_id,
                        principalTable: "retention_policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_processing_activities_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "data_flows",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    processing_activity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    from_it_system_id = table.Column<Guid>(type: "uuid", nullable: true),
                    from_data_collection_source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    from_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    to_it_system_id = table.Column<Guid>(type: "uuid", nullable: true),
                    to_processor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    to_recipient_id = table.Column<Guid>(type: "uuid", nullable: true),
                    to_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    transfer_mechanism = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    is_cross_border = table.Column<bool>(type: "boolean", nullable: false),
                    cross_border_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("pk_data_flows", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_flows_data_categories_data_category_id",
                        column: x => x.data_category_id,
                        principalTable: "data_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_flows_data_collection_sources_from_data_collection_sou",
                        column: x => x.from_data_collection_source_id,
                        principalTable: "data_collection_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_flows_it_systems_from_it_system_id",
                        column: x => x.from_it_system_id,
                        principalTable: "it_systems",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_flows_it_systems_to_it_system_id",
                        column: x => x.to_it_system_id,
                        principalTable: "it_systems",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_flows_processing_activities_processing_activity_id",
                        column: x => x.processing_activity_id,
                        principalTable: "processing_activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_flows_processors_to_processor_id",
                        column: x => x.to_processor_id,
                        principalTable: "processors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_data_flows_recipients_to_recipient_id",
                        column: x => x.to_recipient_id,
                        principalTable: "recipients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "processing_activity_data_categories",
                columns: table => new
                {
                    data_categories_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processing_activity_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processing_activity_data_categories", x => new { x.data_categories_id, x.processing_activity_id });
                    table.ForeignKey(
                        name: "fk_processing_activity_data_categories_data_categories_data_ca",
                        column: x => x.data_categories_id,
                        principalTable: "data_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_processing_activity_data_categories_processing_activities_p",
                        column: x => x.processing_activity_id,
                        principalTable: "processing_activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "processing_activity_data_collection_sources",
                columns: table => new
                {
                    data_collection_sources_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processing_activity_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processing_activity_data_collection_sources", x => new { x.data_collection_sources_id, x.processing_activity_id });
                    table.ForeignKey(
                        name: "fk_processing_activity_data_collection_sources_data_collection",
                        column: x => x.data_collection_sources_id,
                        principalTable: "data_collection_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_processing_activity_data_collection_sources_processing_acti",
                        column: x => x.processing_activity_id,
                        principalTable: "processing_activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "processing_activity_it_systems",
                columns: table => new
                {
                    it_systems_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processing_activity_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processing_activity_it_systems", x => new { x.it_systems_id, x.processing_activity_id });
                    table.ForeignKey(
                        name: "fk_processing_activity_it_systems_it_systems_it_systems_id",
                        column: x => x.it_systems_id,
                        principalTable: "it_systems",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_processing_activity_it_systems_processing_activities_proces",
                        column: x => x.processing_activity_id,
                        principalTable: "processing_activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "processing_activity_processors",
                columns: table => new
                {
                    processing_activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processors_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processing_activity_processors", x => new { x.processing_activity_id, x.processors_id });
                    table.ForeignKey(
                        name: "fk_processing_activity_processors_processing_activities_proces",
                        column: x => x.processing_activity_id,
                        principalTable: "processing_activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_processing_activity_processors_processors_processors_id",
                        column: x => x.processors_id,
                        principalTable: "processors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "processing_activity_recipients",
                columns: table => new
                {
                    processing_activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipients_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processing_activity_recipients", x => new { x.processing_activity_id, x.recipients_id });
                    table.ForeignKey(
                        name: "fk_processing_activity_recipients_processing_activities_proces",
                        column: x => x.processing_activity_id,
                        principalTable: "processing_activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_processing_activity_recipients_recipients_recipients_id",
                        column: x => x.recipients_id,
                        principalTable: "recipients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "created_at", "description", "key", "module" },
                values: new object[,]
                {
                    { new Guid("08815133-dac6-245d-7274-068e7f9a205d"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Create and edit data flow metadata.", "dataflows.manage", "DataInventory" },
                    { new Guid("243eff4f-1c36-fa22-3a66-19fb2cc001f2"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Create and edit data inventory items and their catalogues.", "datainventory.manage", "DataInventory" },
                    { new Guid("42982911-79bd-c1ef-7ed1-5465cf271d90"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Review a submitted processing activity.", "processingactivities.review", "DataInventory" },
                    { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View data flow metadata.", "dataflows.read", "DataInventory" },
                    { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View the processing activity register.", "processingactivities.read", "DataInventory" },
                    { new Guid("993a3c39-c6ca-c17e-6d68-daec524bdf71"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Approve a reviewed processing activity.", "processingactivities.approve", "DataInventory" },
                    { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "View the data inventory and its catalogues (categories, systems, sources, processors, recipients, retention policies).", "datainventory.read", "DataInventory" },
                    { new Guid("c6f595bd-0837-aaca-d5fb-81c739ee4a91"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Create and edit processing activities.", "processingactivities.manage", "DataInventory" }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { new Guid("08815133-dac6-245d-7274-068e7f9a205d"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("243eff4f-1c36-fa22-3a66-19fb2cc001f2"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("42982911-79bd-c1ef-7ed1-5465cf271d90"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("993a3c39-c6ca-c17e-6d68-daec524bdf71"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("c6f595bd-0837-aaca-d5fb-81c739ee4a91"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("08815133-dac6-245d-7274-068e7f9a205d"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("243eff4f-1c36-fa22-3a66-19fb2cc001f2"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("c6f595bd-0837-aaca-d5fb-81c739ee4a91"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") },
                    { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") },
                    { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") },
                    { new Guid("42982911-79bd-c1ef-7ed1-5465cf271d90"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("993a3c39-c6ca-c17e-6d68-daec524bdf71"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") },
                    { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") },
                    { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") },
                    { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") },
                    { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") },
                    { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") },
                    { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") },
                    { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_data_categories_organisation_id",
                table: "data_categories",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_collection_sources_organisation_id",
                table: "data_collection_sources",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_flows_data_category_id",
                table: "data_flows",
                column: "data_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_flows_from_data_collection_source_id",
                table: "data_flows",
                column: "from_data_collection_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_flows_from_it_system_id",
                table: "data_flows",
                column: "from_it_system_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_flows_organisation_id",
                table: "data_flows",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_flows_organisation_id_sequence_number",
                table: "data_flows",
                columns: new[] { "organisation_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_data_flows_processing_activity_id",
                table: "data_flows",
                column: "processing_activity_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_flows_to_it_system_id",
                table: "data_flows",
                column: "to_it_system_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_flows_to_processor_id",
                table: "data_flows",
                column: "to_processor_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_flows_to_recipient_id",
                table: "data_flows",
                column: "to_recipient_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_inventory_items_data_category_id",
                table: "data_inventory_items",
                column: "data_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_inventory_items_data_collection_source_id",
                table: "data_inventory_items",
                column: "data_collection_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_inventory_items_discovered_data_element_id",
                table: "data_inventory_items",
                column: "discovered_data_element_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_inventory_items_it_system_id",
                table: "data_inventory_items",
                column: "it_system_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_inventory_items_organisation_id",
                table: "data_inventory_items",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_inventory_items_organisation_id_risk_level",
                table: "data_inventory_items",
                columns: new[] { "organisation_id", "risk_level" });

            migrationBuilder.CreateIndex(
                name: "ix_data_inventory_items_organisation_id_sequence_number",
                table: "data_inventory_items",
                columns: new[] { "organisation_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_data_inventory_items_owner_user_id",
                table: "data_inventory_items",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_inventory_items_processor_id",
                table: "data_inventory_items",
                column: "processor_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_inventory_items_retention_policy_id",
                table: "data_inventory_items",
                column: "retention_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_it_systems_organisation_id",
                table: "it_systems",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_it_systems_owner_user_id",
                table: "it_systems",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_processing_activities_organisation_id",
                table: "processing_activities",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_processing_activities_organisation_id_sequence_number",
                table: "processing_activities",
                columns: new[] { "organisation_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_processing_activities_organisation_id_status",
                table: "processing_activities",
                columns: new[] { "organisation_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_processing_activities_owner_user_id",
                table: "processing_activities",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_processing_activities_retention_policy_id",
                table: "processing_activities",
                column: "retention_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_processing_activity_data_categories_processing_activity_id",
                table: "processing_activity_data_categories",
                column: "processing_activity_id");

            migrationBuilder.CreateIndex(
                name: "ix_processing_activity_data_collection_sources_processing_acti",
                table: "processing_activity_data_collection_sources",
                column: "processing_activity_id");

            migrationBuilder.CreateIndex(
                name: "ix_processing_activity_it_systems_processing_activity_id",
                table: "processing_activity_it_systems",
                column: "processing_activity_id");

            migrationBuilder.CreateIndex(
                name: "ix_processing_activity_processors_processors_id",
                table: "processing_activity_processors",
                column: "processors_id");

            migrationBuilder.CreateIndex(
                name: "ix_processing_activity_recipients_recipients_id",
                table: "processing_activity_recipients",
                column: "recipients_id");

            migrationBuilder.CreateIndex(
                name: "ix_processors_organisation_id",
                table: "processors",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_recipients_organisation_id",
                table: "recipients",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_retention_policies_organisation_id",
                table: "retention_policies",
                column: "organisation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_flows");

            migrationBuilder.DropTable(
                name: "data_inventory_items");

            migrationBuilder.DropTable(
                name: "processing_activity_data_categories");

            migrationBuilder.DropTable(
                name: "processing_activity_data_collection_sources");

            migrationBuilder.DropTable(
                name: "processing_activity_it_systems");

            migrationBuilder.DropTable(
                name: "processing_activity_processors");

            migrationBuilder.DropTable(
                name: "processing_activity_recipients");

            migrationBuilder.DropTable(
                name: "data_categories");

            migrationBuilder.DropTable(
                name: "data_collection_sources");

            migrationBuilder.DropTable(
                name: "it_systems");

            migrationBuilder.DropTable(
                name: "processors");

            migrationBuilder.DropTable(
                name: "processing_activities");

            migrationBuilder.DropTable(
                name: "recipients");

            migrationBuilder.DropTable(
                name: "retention_policies");

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("08815133-dac6-245d-7274-068e7f9a205d"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("243eff4f-1c36-fa22-3a66-19fb2cc001f2"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("42982911-79bd-c1ef-7ed1-5465cf271d90"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("993a3c39-c6ca-c17e-6d68-daec524bdf71"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c6f595bd-0837-aaca-d5fb-81c739ee4a91"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("08815133-dac6-245d-7274-068e7f9a205d"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("243eff4f-1c36-fa22-3a66-19fb2cc001f2"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("c6f595bd-0837-aaca-d5fb-81c739ee4a91"), new Guid("087fab86-e2f5-4349-5f7b-48ee28bc751f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("0c0ea005-06ac-bbdc-08b1-7f0ab269cf9b") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("42982911-79bd-c1ef-7ed1-5465cf271d90"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("993a3c39-c6ca-c17e-6d68-daec524bdf71"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("3425d982-2f29-47ea-46bd-c2ad128e542a") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("5ae77166-b24b-7aa3-eccb-ad8a4106b434") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("69fa55a0-5741-9bc0-4e51-c0834d45bc66") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("826bf6be-b10e-9fa5-f81e-2c42fa12eb7f") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("b98e94e4-9a76-5548-996c-e66e1750e203") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("5d2e8768-1e83-7378-e439-07c27496b599"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"), new Guid("d3006dac-83c9-c7fb-25eb-66f36565a3cc") });

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("08815133-dac6-245d-7274-068e7f9a205d"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("243eff4f-1c36-fa22-3a66-19fb2cc001f2"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("42982911-79bd-c1ef-7ed1-5465cf271d90"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("5d2e8768-1e83-7378-e439-07c27496b599"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("88d964ea-54d9-c3e6-90f3-4c9a4ab2412d"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("993a3c39-c6ca-c17e-6d68-daec524bdf71"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("a42ea50f-af4e-c348-7e33-aa1cc0450767"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("c6f595bd-0837-aaca-d5fb-81c739ee4a91"));
        }
    }
}
