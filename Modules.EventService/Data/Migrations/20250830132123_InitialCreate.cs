using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.EventService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "events");

            migrationBuilder.CreateTable(
                name: "app_events",
                schema: "events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    OrganizerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_events_Category",
                schema: "events",
                table: "app_events",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_app_events_CreatedAt",
                schema: "events",
                table: "app_events",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_app_events_IsActive",
                schema: "events",
                table: "app_events",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_app_events_IsPublic",
                schema: "events",
                table: "app_events",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_app_events_IsPublished",
                schema: "events",
                table: "app_events",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_app_events_OrganizerId",
                schema: "events",
                table: "app_events",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_app_events_StartDate",
                schema: "events",
                table: "app_events",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_app_events_Status",
                schema: "events",
                table: "app_events",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_app_events_TeamId",
                schema: "events",
                table: "app_events",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_events",
                schema: "events");
        }
    }
}
