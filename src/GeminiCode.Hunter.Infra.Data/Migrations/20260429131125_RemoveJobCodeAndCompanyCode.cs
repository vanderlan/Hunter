using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeminiCode.Hunter.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJobCodeAndCompanyCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobPosition_CompanyCode",
                table: "JobPosition");

            migrationBuilder.DropIndex(
                name: "IX_JobPosition_JobCode",
                table: "JobPosition");

            migrationBuilder.DropColumn(
                name: "CompanyCode",
                table: "JobPosition");

            migrationBuilder.DropColumn(
                name: "JobCode",
                table: "JobPosition");

            migrationBuilder.DropColumn(
                name: "CompanyCode",
                table: "CvAnalysisMatch");

            migrationBuilder.DropColumn(
                name: "JobCode",
                table: "CvAnalysisMatch");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                table: "JobPosition",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobCode",
                table: "JobPosition",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                table: "CvAnalysisMatch",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobCode",
                table: "CvAnalysisMatch",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosition_CompanyCode",
                table: "JobPosition",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosition_JobCode",
                table: "JobPosition",
                column: "JobCode");
        }
    }
}
