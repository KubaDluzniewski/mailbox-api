using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class siema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActivationTokens_Users_UserId",
                table: "UserActivationTokens");

            migrationBuilder.DropIndex(
                name: "IX_UserActivationTokens_UserId",
                table: "UserActivationTokens");

            migrationBuilder.AddColumn<string>(
                name: "NewEmail",
                table: "UserActivationTokens",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewEmail",
                table: "UserActivationTokens");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivationTokens_UserId",
                table: "UserActivationTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivationTokens_Users_UserId",
                table: "UserActivationTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
