using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GeminiCode.Hunter.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHiringProcessEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HiringProcess",
                columns: table => new
                {
                    HiringProcessId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CandidateId = table.Column<long>(type: "bigint", nullable: false),
                    JobPositionId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringProcess", x => x.HiringProcessId);
                    table.ForeignKey(
                        name: "FK_HiringProcess_Candidate_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidate",
                        principalColumn: "CandidateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringProcess_JobPosition_JobPositionId",
                        column: x => x.JobPositionId,
                        principalTable: "JobPosition",
                        principalColumn: "JobPositionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringProcess_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringProcess_User_LastUpdatedByUserId",
                        column: x => x.LastUpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HiringProcessNote",
                columns: table => new
                {
                    HiringProcessNoteId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HiringProcessId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PublicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringProcessNote", x => x.HiringProcessNoteId);
                    table.ForeignKey(
                        name: "FK_HiringProcessNote_HiringProcess_HiringProcessId",
                        column: x => x.HiringProcessId,
                        principalTable: "HiringProcess",
                        principalColumn: "HiringProcessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HiringProcessNote_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HiringProcessStatusHistory",
                columns: table => new
                {
                    HiringProcessStatusHistoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HiringProcessId = table.Column<long>(type: "bigint", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: false),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    ChangedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PublicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringProcessStatusHistory", x => x.HiringProcessStatusHistoryId);
                    table.ForeignKey(
                        name: "FK_HiringProcessStatusHistory_HiringProcess_HiringProcessId",
                        column: x => x.HiringProcessId,
                        principalTable: "HiringProcess",
                        principalColumn: "HiringProcessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HiringProcessStatusHistory_User_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcess_CandidateId",
                table: "HiringProcess",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcess_CandidateId_JobPositionId",
                table: "HiringProcess",
                columns: new[] { "CandidateId", "JobPositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcess_CreatedByUserId",
                table: "HiringProcess",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcess_JobPositionId",
                table: "HiringProcess",
                column: "JobPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcess_LastUpdatedByUserId",
                table: "HiringProcess",
                column: "LastUpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcess_PublicId",
                table: "HiringProcess",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcess_Status",
                table: "HiringProcess",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcessNote_CreatedAt",
                table: "HiringProcessNote",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcessNote_HiringProcessId",
                table: "HiringProcessNote",
                column: "HiringProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcessNote_PublicId",
                table: "HiringProcessNote",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcessNote_UserId",
                table: "HiringProcessNote",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcessStatusHistory_ChangedByUserId",
                table: "HiringProcessStatusHistory",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcessStatusHistory_CreatedAt",
                table: "HiringProcessStatusHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcessStatusHistory_HiringProcessId",
                table: "HiringProcessStatusHistory",
                column: "HiringProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringProcessStatusHistory_PublicId",
                table: "HiringProcessStatusHistory",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HiringProcessNote");

            migrationBuilder.DropTable(
                name: "HiringProcessStatusHistory");

            migrationBuilder.DropTable(
                name: "HiringProcess");
        }
    }
}
