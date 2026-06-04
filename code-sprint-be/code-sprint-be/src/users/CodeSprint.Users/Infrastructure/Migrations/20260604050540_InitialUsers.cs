using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSprint.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    handle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    avatar = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    provider_subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    member_since = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "users",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_handle",
                schema: "users",
                table: "users",
                column: "handle",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_provider_provider_subject",
                schema: "users",
                table: "users",
                columns: new[] { "provider", "provider_subject" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users",
                schema: "users");
        }
    }
}
