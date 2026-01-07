using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GeminiCode.Hunter.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPosition",
                columns: table => new
                {
                    JobPositionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExternalUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CompanyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    JobCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    Responsibilities = table.Column<string>(type: "text", nullable: true),
                    Requirements = table.Column<string>(type: "text", nullable: true),
                    MustHaveSkills = table.Column<string>(type: "text", nullable: true),
                    NiceToHaveSkills = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastCheckedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPosition", x => x.JobPositionId);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Refreshtoken = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PublicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Refreshtoken);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Profile = table.Column<int>(type: "integer", nullable: false),
                    Password = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PublicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "UserId", "CreatedAt", "Email", "LastUpdated", "Name", "Password", "Profile", "PublicId" },
                values: new object[] { 923165498765432123L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "adm@hunter.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hunter Admin User", "3c9909afec25354d551dae21590bb26e38d53f2173b8d3dc3eee4c047e7ab1c1eb8b85103e3be7ba613b31bb5c9c36214dc9f14a42fd7a2fdb84856bca5c44c2", 1, "16537902-1ca7-49ca-82e5-0be137f9aeeb" });

            migrationBuilder.CreateIndex(
                name: "IX_JobPosition_CompanyCode",
                table: "JobPosition",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosition_ExternalUrl",
                table: "JobPosition",
                column: "ExternalUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPosition_IsActive",
                table: "JobPosition",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosition_JobCode",
                table: "JobPosition",
                column: "JobCode");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosition_PublicId",
                table: "JobPosition",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_Email",
                table: "RefreshToken",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_PublicId",
                table: "RefreshToken",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_PublicId",
                table: "User",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPosition");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
