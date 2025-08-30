using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.PaymentService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "payments");

            migrationBuilder.CreateTable(
                name: "app_payments",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Gateway = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GatewayMetadata = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_payments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "app_payment_items",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Ticket"),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_payment_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_app_payment_items_app_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "payments",
                        principalTable: "app_payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_payment_items_CreatedAt",
                schema: "payments",
                table: "app_payment_items",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_app_payment_items_IsActive",
                schema: "payments",
                table: "app_payment_items",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_app_payment_items_ItemId",
                schema: "payments",
                table: "app_payment_items",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_app_payment_items_ItemType",
                schema: "payments",
                table: "app_payment_items",
                column: "ItemType");

            migrationBuilder.CreateIndex(
                name: "IX_app_payment_items_PaymentId",
                schema: "payments",
                table: "app_payment_items",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_app_payments_CompletedAt",
                schema: "payments",
                table: "app_payments",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_app_payments_CreatedAt",
                schema: "payments",
                table: "app_payments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_app_payments_EventId",
                schema: "payments",
                table: "app_payments",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_app_payments_Gateway",
                schema: "payments",
                table: "app_payments",
                column: "Gateway");

            migrationBuilder.CreateIndex(
                name: "IX_app_payments_IsActive",
                schema: "payments",
                table: "app_payments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_app_payments_reference_unique",
                schema: "payments",
                table: "app_payments",
                column: "PaymentReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_app_payments_Status",
                schema: "payments",
                table: "app_payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_app_payments_UserId",
                schema: "payments",
                table: "app_payments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_payment_items",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "app_payments",
                schema: "payments");
        }
    }
}
