using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSprint.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EmailNotUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_email",
                schema: "users",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "users",
                table: "users",
                column: "email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_email",
                schema: "users",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "users",
                table: "users",
                column: "email",
                unique: true);
        }
    }
}
