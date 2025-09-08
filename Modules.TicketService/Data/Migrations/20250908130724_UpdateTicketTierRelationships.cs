using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.TicketService.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTicketTierRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_app_tickets_app_ticket_tiers_TicketTierId",
                schema: "tickets",
                table: "app_tickets");

            migrationBuilder.DropIndex(
                name: "IX_app_tickets_TicketTier",
                schema: "tickets",
                table: "app_tickets");

            migrationBuilder.DropColumn(
                name: "TicketTier",
                schema: "tickets",
                table: "app_tickets");

            migrationBuilder.AlterColumn<Guid>(
                name: "TicketTierId",
                schema: "tickets",
                table: "app_tickets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_app_ticket_tiers_event_name_unique",
                schema: "tickets",
                table: "app_ticket_tiers",
                columns: new[] { "EventId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_app_tickets_app_ticket_tiers_TicketTierId",
                schema: "tickets",
                table: "app_tickets",
                column: "TicketTierId",
                principalSchema: "tickets",
                principalTable: "app_ticket_tiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_app_tickets_app_ticket_tiers_TicketTierId",
                schema: "tickets",
                table: "app_tickets");

            migrationBuilder.DropIndex(
                name: "IX_app_ticket_tiers_event_name_unique",
                schema: "tickets",
                table: "app_ticket_tiers");

            migrationBuilder.AlterColumn<Guid>(
                name: "TicketTierId",
                schema: "tickets",
                table: "app_tickets",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "TicketTier",
                schema: "tickets",
                table: "app_tickets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_app_tickets_TicketTier",
                schema: "tickets",
                table: "app_tickets",
                column: "TicketTier");

            migrationBuilder.AddForeignKey(
                name: "FK_app_tickets_app_ticket_tiers_TicketTierId",
                schema: "tickets",
                table: "app_tickets",
                column: "TicketTierId",
                principalSchema: "tickets",
                principalTable: "app_ticket_tiers",
                principalColumn: "Id");
        }
    }
}
