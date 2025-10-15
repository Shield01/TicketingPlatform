using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.EventService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Added_MinimumPriceCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MinimumPriceCurrency",
                schema: "events",
                table: "app_events",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumPriceCurrency",
                schema: "events",
                table: "app_events");
        }
    }
}
