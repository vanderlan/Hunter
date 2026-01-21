using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeminiCode.Hunter.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceSkillsWithRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename Skills column to Requirements
            migrationBuilder.RenameColumn(
                name: "Skills",
                table: "JobPosition",
                newName: "Requirements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rename Requirements column back to Skills
            migrationBuilder.RenameColumn(
                name: "Requirements",
                table: "JobPosition",
                newName: "Skills");
        }
    }
}
