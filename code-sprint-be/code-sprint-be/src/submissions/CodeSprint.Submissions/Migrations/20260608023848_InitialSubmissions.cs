using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSprint.Submissions.Migrations
{
    /// <inheritdoc />
    public partial class InitialSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "submissions");

            migrationBuilder.CreateTable(
                name: "submissions",
                schema: "submissions",
                columns: table => new
                {
                    submission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    problem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    source_code = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    verdict = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    points_awarded = table.Column<int>(type: "integer", nullable: true),
                    runtime_ms = table.Column<int>(type: "integer", nullable: true),
                    memory_kb = table.Column<int>(type: "integer", nullable: true),
                    evaluated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_submissions", x => x.submission_id);
                });

            migrationBuilder.CreateTable(
                name: "submission_test_results",
                schema: "submissions",
                columns: table => new
                {
                    ordinal = table.Column<int>(type: "integer", nullable: false),
                    submission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    runtime_ms = table.Column<int>(type: "integer", nullable: false),
                    memory_kb = table.Column<int>(type: "integer", nullable: false),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false),
                    actual_output = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_submission_test_results", x => new { x.submission_id, x.ordinal });
                    table.ForeignKey(
                        name: "FK_submission_test_results_submissions_submission_id",
                        column: x => x.submission_id,
                        principalSchema: "submissions",
                        principalTable: "submissions",
                        principalColumn: "submission_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_submissions_user_id_submitted_at",
                schema: "submissions",
                table: "submissions",
                columns: new[] { "user_id", "submitted_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "submission_test_results",
                schema: "submissions");

            migrationBuilder.DropTable(
                name: "submissions",
                schema: "submissions");
        }
    }
}
