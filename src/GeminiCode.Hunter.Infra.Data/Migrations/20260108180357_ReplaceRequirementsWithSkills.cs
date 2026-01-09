using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeminiCode.Hunter.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceRequirementsWithSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Requirements",
                table: "JobPosition",
                newName: "Skills");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Skills",
                table: "JobPosition",
                newName: "Requirements");
        }
    }
}
