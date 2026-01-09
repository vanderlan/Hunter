using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeminiCode.Hunter.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReceiveUsersNotificationJobChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReceiveJobChangeNotifications",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "UserId",
                keyValue: 923165498765432123L,
                column: "ReceiveJobChangeNotifications",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiveJobChangeNotifications",
                table: "User");
        }
    }
}
