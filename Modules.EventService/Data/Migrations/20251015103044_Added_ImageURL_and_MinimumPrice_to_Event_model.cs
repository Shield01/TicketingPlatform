using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.EventService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Added_ImageURL_and_MinimumPrice_to_Event_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "events",
                table: "app_events",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumPrice",
                schema: "events",
                table: "app_events",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "events",
                table: "app_events");

            migrationBuilder.DropColumn(
                name: "MinimumPrice",
                schema: "events",
                table: "app_events");
        }
    }
}
