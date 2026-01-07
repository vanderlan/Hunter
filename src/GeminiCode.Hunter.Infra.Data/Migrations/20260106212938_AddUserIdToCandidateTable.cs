using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeminiCode.Hunter.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToCandidateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Candidate",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Candidate_UserId",
                table: "Candidate",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidate_User_UserId",
                table: "Candidate",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidate_User_UserId",
                table: "Candidate");

            migrationBuilder.DropIndex(
                name: "IX_Candidate_UserId",
                table: "Candidate");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Candidate");
        }
    }
}
