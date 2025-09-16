using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.TicketService.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImplementedTicketStatusOverrideFunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_ticket_audit_logs",
                schema: "tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NewStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AdditionalDetails = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WasForced = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_ticket_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_app_ticket_audit_logs_app_tickets_TicketId",
                        column: x => x.TicketId,
                        principalSchema: "tickets",
                        principalTable: "app_tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_ticket_audit_logs_ActionType",
                schema: "tickets",
                table: "app_ticket_audit_logs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_app_ticket_audit_logs_IsActive",
                schema: "tickets",
                table: "app_ticket_audit_logs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_app_ticket_audit_logs_PerformedAt",
                schema: "tickets",
                table: "app_ticket_audit_logs",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_app_ticket_audit_logs_PerformedByUserId",
                schema: "tickets",
                table: "app_ticket_audit_logs",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_app_ticket_audit_logs_ticket_performed_at",
                schema: "tickets",
                table: "app_ticket_audit_logs",
                columns: new[] { "TicketId", "PerformedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_app_ticket_audit_logs_TicketId",
                schema: "tickets",
                table: "app_ticket_audit_logs",
                column: "TicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_ticket_audit_logs",
                schema: "tickets");
        }
    }
}
