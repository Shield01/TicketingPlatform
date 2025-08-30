using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.TicketService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tickets");

            migrationBuilder.CreateTable(
                name: "app_ticket_tiers",
                schema: "tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    MaxQuantity = table.Column<int>(type: "integer", nullable: false),
                    SoldQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SaleStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SaleEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_ticket_tiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "app_tickets",
                schema: "tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketTier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    TicketCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    QRCodeData = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    TicketTierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_app_tickets_app_ticket_tiers_TicketTierId",
                        column: x => x.TicketTierId,
                        principalSchema: "tickets",
                        principalTable: "app_ticket_tiers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_ticket_tiers_CreatedAt",
                schema: "tickets",
                table: "app_ticket_tiers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_app_ticket_tiers_EventId",
                schema: "tickets",
                table: "app_ticket_tiers",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_app_ticket_tiers_IsActive",
                schema: "tickets",
                table: "app_ticket_tiers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_app_ticket_tiers_IsAvailable",
                schema: "tickets",
                table: "app_ticket_tiers",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_app_ticket_tiers_Price",
                schema: "tickets",
                table: "app_ticket_tiers",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_app_tickets_CreatedAt",
                schema: "tickets",
                table: "app_tickets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_app_tickets_EventId",
                schema: "tickets",
                table: "app_tickets",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_app_tickets_IsActive",
                schema: "tickets",
                table: "app_tickets",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_app_tickets_IsUsed",
                schema: "tickets",
                table: "app_tickets",
                column: "IsUsed");

            migrationBuilder.CreateIndex(
                name: "IX_app_tickets_Status",
                schema: "tickets",
                table: "app_tickets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_app_tickets_ticket_code_unique",
                schema: "tickets",
                table: "app_tickets",
                column: "TicketCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_app_tickets_TicketTier",
                schema: "tickets",
                table: "app_tickets",
                column: "TicketTier");

            migrationBuilder.CreateIndex(
                name: "IX_app_tickets_TicketTierId",
                schema: "tickets",
                table: "app_tickets",
                column: "TicketTierId");

            migrationBuilder.CreateIndex(
                name: "IX_app_tickets_UserId",
                schema: "tickets",
                table: "app_tickets",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_tickets",
                schema: "tickets");

            migrationBuilder.DropTable(
                name: "app_ticket_tiers",
                schema: "tickets");
        }
    }
}
