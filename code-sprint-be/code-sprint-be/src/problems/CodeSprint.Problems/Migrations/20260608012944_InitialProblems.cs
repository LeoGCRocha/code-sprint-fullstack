using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSprint.Problems.Migrations
{
    /// <inheritdoc />
    public partial class InitialProblems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "problems");

            migrationBuilder.CreateTable(
                name: "problems",
                schema: "problems",
                columns: table => new
                {
                    problem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    difficulty = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    points = table.Column<int>(type: "integer", nullable: false),
                    estimated_minutes = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    constraints = table.Column<string>(type: "jsonb", nullable: false),
                    input_format = table.Column<string>(type: "jsonb", nullable: false),
                    notes = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_problems", x => x.problem_id);
                });

            migrationBuilder.CreateTable(
                name: "problem_examples",
                schema: "problems",
                columns: table => new
                {
                    ordinal = table.Column<int>(type: "integer", nullable: false),
                    problem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    input = table.Column<string>(type: "text", nullable: false),
                    output = table.Column<string>(type: "text", nullable: false),
                    explanation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_problem_examples", x => new { x.problem_id, x.ordinal });
                    table.ForeignKey(
                        name: "FK_problem_examples_problems_problem_id",
                        column: x => x.problem_id,
                        principalSchema: "problems",
                        principalTable: "problems",
                        principalColumn: "problem_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "problem_tags",
                schema: "problems",
                columns: table => new
                {
                    tag = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    problem_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_problem_tags", x => new { x.problem_id, x.tag });
                    table.ForeignKey(
                        name: "FK_problem_tags_problems_problem_id",
                        column: x => x.problem_id,
                        principalSchema: "problems",
                        principalTable: "problems",
                        principalColumn: "problem_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "problem_test_cases",
                schema: "problems",
                columns: table => new
                {
                    ordinal = table.Column<int>(type: "integer", nullable: false),
                    problem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    input = table.Column<string>(type: "text", nullable: false),
                    expected_output = table.Column<string>(type: "text", nullable: false),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_problem_test_cases", x => new { x.problem_id, x.ordinal });
                    table.ForeignKey(
                        name: "FK_problem_test_cases_problems_problem_id",
                        column: x => x.problem_id,
                        principalSchema: "problems",
                        principalTable: "problems",
                        principalColumn: "problem_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_problems_slug",
                schema: "problems",
                table: "problems",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "problem_examples",
                schema: "problems");

            migrationBuilder.DropTable(
                name: "problem_tags",
                schema: "problems");

            migrationBuilder.DropTable(
                name: "problem_test_cases",
                schema: "problems");

            migrationBuilder.DropTable(
                name: "problems",
                schema: "problems");
        }
    }
}
