using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeminiCode.Hunter.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionIdToLookerStudioCredential : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "LookerStudioCredential",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "LookerStudioCredential");
        }
    }
}
