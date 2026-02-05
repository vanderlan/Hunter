using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GeminiCode.Hunter.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLookerStudioCredentialsAndJobPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryRestrictions",
                table: "JobPosition",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "JobPosition",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PriorityLastSyncedAt",
                table: "JobPosition",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LookerStudioCredential",
                columns: table => new
                {
                    LookerStudioCredentialId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EncryptedCookieData = table.Column<string>(type: "text", nullable: false),
                    XsrfToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DatasourceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReportId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UpdatedByUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookerStudioCredential", x => x.LookerStudioCredentialId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LookerStudioCredential_IsActive",
                table: "LookerStudioCredential",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LookerStudioCredential_PublicId",
                table: "LookerStudioCredential",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LookerStudioCredential");

            migrationBuilder.DropColumn(
                name: "CountryRestrictions",
                table: "JobPosition");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "JobPosition");

            migrationBuilder.DropColumn(
                name: "PriorityLastSyncedAt",
                table: "JobPosition");
        }
    }
}
