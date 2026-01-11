using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GeminiCode.Hunter.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCvAnalysisHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CvAnalysis",
                columns: table => new
                {
                    CvAnalysisId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CandidateId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPositionsAnalyzed = table.Column<int>(type: "integer", nullable: false),
                    MatchesFound = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CvAnalysis", x => x.CvAnalysisId);
                    table.ForeignKey(
                        name: "FK_CvAnalysis_Candidate_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidate",
                        principalColumn: "CandidateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CvAnalysis_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CvAnalysisMatch",
                columns: table => new
                {
                    CvAnalysisMatchId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CvAnalysisId = table.Column<long>(type: "bigint", nullable: false),
                    JobPositionId = table.Column<long>(type: "bigint", nullable: false),
                    MatchPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MatchReason = table.Column<string>(type: "text", nullable: false),
                    MatchingSkills = table.Column<string>(type: "text", nullable: false),
                    MissingSkills = table.Column<string>(type: "text", nullable: false),
                    JobTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JobCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CompanyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    JobSummary = table.Column<string>(type: "text", nullable: true),
                    MustHaveSkills = table.Column<string>(type: "text", nullable: true),
                    NiceToHaveSkills = table.Column<string>(type: "text", nullable: true),
                    PublicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CvAnalysisMatch", x => x.CvAnalysisMatchId);
                    table.ForeignKey(
                        name: "FK_CvAnalysisMatch_CvAnalysis_CvAnalysisId",
                        column: x => x.CvAnalysisId,
                        principalTable: "CvAnalysis",
                        principalColumn: "CvAnalysisId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CvAnalysisMatch_JobPosition_JobPositionId",
                        column: x => x.JobPositionId,
                        principalTable: "JobPosition",
                        principalColumn: "JobPositionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CvAnalysis_AnalyzedAt",
                table: "CvAnalysis",
                column: "AnalyzedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_CvAnalysis_CandidateId",
                table: "CvAnalysis",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_CvAnalysis_PublicId",
                table: "CvAnalysis",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CvAnalysis_UserId",
                table: "CvAnalysis",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CvAnalysisMatch_CvAnalysisId",
                table: "CvAnalysisMatch",
                column: "CvAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_CvAnalysisMatch_JobPositionId",
                table: "CvAnalysisMatch",
                column: "JobPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_CvAnalysisMatch_PublicId",
                table: "CvAnalysisMatch",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CvAnalysisMatch");

            migrationBuilder.DropTable(
                name: "CvAnalysis");
        }
    }
}
