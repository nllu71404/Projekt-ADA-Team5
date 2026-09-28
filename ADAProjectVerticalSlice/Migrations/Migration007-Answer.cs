using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114253_Answer")]
    public class Migration007_Answer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Answers
            migrationBuilder.CreateTable(
                name: "Answer",
                columns: table => new
                {
                    AnswerId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Point = table.Column<int>(type: "integer", nullable: false), 
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answer", λ => λ.AnswerId);

                    table.ForeignKey(
                    name: "FK_Answer_Question_QuestionId",
                    column: λ => λ.QuestionId,
                    principalTable: "Question",
                    principalColumn: "QuestionId",
                    onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answer");
        }
    }
}
