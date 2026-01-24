using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReadStatusAgain : Migration
    {
        /// <inheritdoc />
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure no NULLs exist before potential constraints
            migrationBuilder.Sql(@"UPDATE ""MessageRecipients"" SET ""IsRead"" = false WHERE ""IsRead"" IS NULL");

            // Optional: If existing column is nullable, you might want to alter it to NOT NULL
            // migrationBuilder.AlterColumn<bool>(...);
            // But since we are just syncing, raw SQL update is safer to fix runtime errors.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Do nothing as Up did nothing.
        }
    }
}
