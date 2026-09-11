using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DPDP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AssessmentsReviewPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "created_at", "description", "key", "module" },
                values: new object[] { new Guid("9b7ab8d1-8189-09c9-306c-cd4a9c20b9ac"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Review submitted compliance assessments.", "assessments.review", "Assessments" });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { new Guid("9b7ab8d1-8189-09c9-306c-cd4a9c20b9ac"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") },
                    { new Guid("9b7ab8d1-8189-09c9-306c-cd4a9c20b9ac"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("9b7ab8d1-8189-09c9-306c-cd4a9c20b9ac"), new Guid("016c7d6f-109e-eed9-173a-a65559a34eca") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("9b7ab8d1-8189-09c9-306c-cd4a9c20b9ac"), new Guid("2ee608de-7ae0-64a2-ed3b-8da1f985c363") });

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("9b7ab8d1-8189-09c9-306c-cd4a9c20b9ac"));
        }
    }
}
