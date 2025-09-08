using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.TicketService.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedTicketmodeltoincludehelpermethodsandticketcodegeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "tickets",
                table: "app_tickets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "UNUSED",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Active");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                schema: "tickets",
                table: "app_tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_app_tickets_PaymentId",
                schema: "tickets",
                table: "app_tickets",
                column: "PaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_app_tickets_PaymentId",
                schema: "tickets",
                table: "app_tickets");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                schema: "tickets",
                table: "app_tickets");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "tickets",
                table: "app_tickets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "UNUSED");
        }
    }
}
