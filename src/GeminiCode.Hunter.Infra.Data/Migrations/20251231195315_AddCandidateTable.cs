using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GeminiCode.Hunter.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Candidate",
                columns: table => new
                {
                    CandidateId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CvText = table.Column<string>(type: "text", nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidate", x => x.CandidateId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candidate_AnalyzedAt",
                table: "Candidate",
                column: "AnalyzedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Candidate_Email",
                table: "Candidate",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Candidate_PublicId",
                table: "Candidate",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Candidate");
        }
    }
}
