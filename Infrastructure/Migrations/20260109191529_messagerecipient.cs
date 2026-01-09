using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class messagerecipient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageRecipients_Users_UserId",
                table: "MessageRecipients");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "MessageRecipients",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "RecipientEntityId",
                table: "MessageRecipients",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecipientType",
                table: "MessageRecipients",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageRecipients_Users_UserId",
                table: "MessageRecipients",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageRecipients_Users_UserId",
                table: "MessageRecipients");

            migrationBuilder.DropColumn(
                name: "RecipientEntityId",
                table: "MessageRecipients");

            migrationBuilder.DropColumn(
                name: "RecipientType",
                table: "MessageRecipients");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "MessageRecipients",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageRecipients_Users_UserId",
                table: "MessageRecipients",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
