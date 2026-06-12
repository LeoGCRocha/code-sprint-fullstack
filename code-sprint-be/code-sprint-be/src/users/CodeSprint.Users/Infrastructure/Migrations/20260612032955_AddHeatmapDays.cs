using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSprint.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHeatmapDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "heatmap_days",
                schema: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_heatmap_days", x => new { x.user_id, x.day });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "heatmap_days",
                schema: "users");
        }
    }
}
