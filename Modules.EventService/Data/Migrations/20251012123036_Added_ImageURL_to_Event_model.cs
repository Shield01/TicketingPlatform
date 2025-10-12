using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.EventService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Added_ImageURL_to_Event_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                schema: "events",
                table: "app_events",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageURL",
                schema: "events",
                table: "app_events");
        }
    }
}
