using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GeminiCode.Hunter.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicJobBoardAndReferralRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicCode",
                table: "JobPosition",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublicContentGeneratedAt",
                table: "JobPosition",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicMustHaveSkills",
                table: "JobPosition",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicNiceToHaveSkills",
                table: "JobPosition",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicRequirements",
                table: "JobPosition",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicResponsibilities",
                table: "JobPosition",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicSummary",
                table: "JobPosition",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "CvAnalysis",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Candidate",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmationSentAt",
                table: "Candidate",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationToken",
                table: "Candidate",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmedAt",
                table: "Candidate",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAcceptedTerms",
                table: "Candidate",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailConfirmed",
                table: "Candidate",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PublicToken",
                table: "Candidate",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReceiveJobMatchNotifications",
                table: "Candidate",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "Candidate",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "TermsAcceptedAt",
                table: "Candidate",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReferralRequest",
                columns: table => new
                {
                    ReferralRequestId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CandidateId = table.Column<long>(type: "bigint", nullable: false),
                    JobPositionId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MatchPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ResolvedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ResolutionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HiringProcessId = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralRequest", x => x.ReferralRequestId);
                    table.ForeignKey(
                        name: "FK_ReferralRequest_Candidate_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidate",
                        principalColumn: "CandidateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReferralRequest_HiringProcess_HiringProcessId",
                        column: x => x.HiringProcessId,
                        principalTable: "HiringProcess",
                        principalColumn: "HiringProcessId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReferralRequest_JobPosition_JobPositionId",
                        column: x => x.JobPositionId,
                        principalTable: "JobPosition",
                        principalColumn: "JobPositionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReferralRequest_User_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            // Existing rows received the empty default above; give each one a unique
            // public code before the unique index is created. New rows get theirs in code.
            migrationBuilder.Sql("""
                UPDATE "JobPosition"
                SET "PublicCode" = substr(md5(random()::text || "JobPositionId"::text), 1, 10)
                WHERE "PublicCode" = '' OR "PublicCode" IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_JobPosition_PublicCode",
                table: "JobPosition",
                column: "PublicCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Candidate_EmailConfirmationToken",
                table: "Candidate",
                column: "EmailConfirmationToken");

            migrationBuilder.CreateIndex(
                name: "IX_Candidate_PublicToken",
                table: "Candidate",
                column: "PublicToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Candidate_Source",
                table: "Candidate",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRequest_CandidateId_JobPositionId",
                table: "ReferralRequest",
                columns: new[] { "CandidateId", "JobPositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRequest_HiringProcessId",
                table: "ReferralRequest",
                column: "HiringProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRequest_JobPositionId",
                table: "ReferralRequest",
                column: "JobPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRequest_PublicId",
                table: "ReferralRequest",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRequest_ResolvedByUserId",
                table: "ReferralRequest",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRequest_Status",
                table: "ReferralRequest",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferralRequest");

            migrationBuilder.DropIndex(
                name: "IX_JobPosition_PublicCode",
                table: "JobPosition");

            migrationBuilder.DropIndex(
                name: "IX_Candidate_EmailConfirmationToken",
                table: "Candidate");

            migrationBuilder.DropIndex(
                name: "IX_Candidate_PublicToken",
                table: "Candidate");

            migrationBuilder.DropIndex(
                name: "IX_Candidate_Source",
                table: "Candidate");

            migrationBuilder.DropColumn(
                name: "PublicCode",
                table: "JobPosition");

            migrationBuilder.DropColumn(
                name: "PublicContentGeneratedAt",
                table: "JobPosition");

            migrationBuilder.DropColumn(
                name: "PublicMustHaveSkills",
                table: "JobPosition");

            migrationBuilder.DropColumn(
                name: "PublicNiceToHaveSkills",
                table: "JobPosition");

            migrationBuilder.DropColumn(
                name: "PublicRequirements",
                table: "JobPosition");

            migrationBuilder.DropColumn(
                name: "PublicResponsibilities",
                table: "JobPosition");

            migrationBuilder.DropColumn(
                name: "PublicSummary",
                table: "JobPosition");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationSentAt",
                table: "Candidate");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationToken",
                table: "Candidate");

            migrationBuilder.DropColumn(
                name: "EmailConfirmedAt",
                table: "Candidate");

            migrationBuilder.DropColumn(
                name: "HasAcceptedTerms",
                table: "Candidate");

            migrationBuilder.DropColumn(
                name: "IsEmailConfirmed",
                table: "Candidate");

            migrationBuilder.DropColumn(
                name: "PublicToken",
                table: "Candidate");

            migrationBuilder.DropColumn(
                name: "ReceiveJobMatchNotifications",
                table: "Candidate");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Candidate");

            migrationBuilder.DropColumn(
                name: "TermsAcceptedAt",
                table: "Candidate");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "CvAnalysis",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Candidate",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
