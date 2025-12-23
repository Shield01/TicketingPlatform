using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.PaymentService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPayoutTransactionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastWebhookEventId",
                schema: "payments",
                table: "app_payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWebhookReceivedAt",
                schema: "payments",
                table: "app_payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WebhookCount",
                schema: "payments",
                table: "app_payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "app_payout_transactions",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "NGN"),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BankCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Narration = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "INITIATED"),
                    Gateway = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "PayAza"),
                    GatewayTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GatewayFee = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    GatewayMetadata = table.Column<string>(type: "jsonb", nullable: true),
                    IsDryRun = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_payout_transactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_payout_transactions_AccountNumber",
                schema: "payments",
                table: "app_payout_transactions",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_app_payout_transactions_BankCode",
                schema: "payments",
                table: "app_payout_transactions",
                column: "BankCode");

            migrationBuilder.CreateIndex(
                name: "IX_app_payout_transactions_CompletedAt",
                schema: "payments",
                table: "app_payout_transactions",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_app_payout_transactions_CreatedAt",
                schema: "payments",
                table: "app_payout_transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_app_payout_transactions_EventId",
                schema: "payments",
                table: "app_payout_transactions",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_app_payout_transactions_Gateway",
                schema: "payments",
                table: "app_payout_transactions",
                column: "Gateway");

            migrationBuilder.CreateIndex(
                name: "IX_app_payout_transactions_InitiatedByUserId",
                schema: "payments",
                table: "app_payout_transactions",
                column: "InitiatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_app_payout_transactions_IsActive",
                schema: "payments",
                table: "app_payout_transactions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_app_payout_transactions_IsDryRun",
                schema: "payments",
                table: "app_payout_transactions",
                column: "IsDryRun");

            migrationBuilder.CreateIndex(
                name: "IX_app_payout_transactions_RecipientUserId",
                schema: "payments",
                table: "app_payout_transactions",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_app_payout_transactions_reference_unique",
                schema: "payments",
                table: "app_payout_transactions",
                column: "TransactionReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_app_payout_transactions_Status",
                schema: "payments",
                table: "app_payout_transactions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_payout_transactions",
                schema: "payments");

            migrationBuilder.DropColumn(
                name: "LastWebhookEventId",
                schema: "payments",
                table: "app_payments");

            migrationBuilder.DropColumn(
                name: "LastWebhookReceivedAt",
                schema: "payments",
                table: "app_payments");

            migrationBuilder.DropColumn(
                name: "WebhookCount",
                schema: "payments",
                table: "app_payments");
        }
    }
}
