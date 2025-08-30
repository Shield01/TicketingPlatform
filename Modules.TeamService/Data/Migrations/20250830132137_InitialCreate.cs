using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.TeamService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "teams");

            migrationBuilder.CreateTable(
                name: "User",
                schema: "teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "app_teams",
                schema: "teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TeamLeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_app_teams_User_TeamLeaderId",
                        column: x => x.TeamLeaderId,
                        principalSchema: "teams",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "app_team_members",
                schema: "teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_team_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_app_team_members_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "teams",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_app_team_members_app_teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "teams",
                        principalTable: "app_teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_team_members_CreatedAt",
                schema: "teams",
                table: "app_team_members",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_app_team_members_IsActive",
                schema: "teams",
                table: "app_team_members",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_app_team_members_team_user_unique",
                schema: "teams",
                table: "app_team_members",
                columns: new[] { "TeamId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_app_team_members_TeamId",
                schema: "teams",
                table: "app_team_members",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_app_team_members_TeamRole",
                schema: "teams",
                table: "app_team_members",
                column: "TeamRole");

            migrationBuilder.CreateIndex(
                name: "IX_app_team_members_UserId",
                schema: "teams",
                table: "app_team_members",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_app_teams_CreatedAt",
                schema: "teams",
                table: "app_teams",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_app_teams_IsActive",
                schema: "teams",
                table: "app_teams",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_app_teams_Name",
                schema: "teams",
                table: "app_teams",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_app_teams_TeamLeaderId",
                schema: "teams",
                table: "app_teams",
                column: "TeamLeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_team_members",
                schema: "teams");

            migrationBuilder.DropTable(
                name: "app_teams",
                schema: "teams");

            migrationBuilder.DropTable(
                name: "User",
                schema: "teams");
        }
    }
}
