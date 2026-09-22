using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919115500_Answer")]
    public class Migration015_Answer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Answers
            migrationBuilder.CreateTable(
                name: "Answer",
                columns: table => new
                {
                    AnswerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answer", λ => λ.AnswerId);

                    table.ForeignKey(
                    name: "FK_Answer_Responses_ResponseId",
                    column: λ => λ.ResponseId,
                    principalTable: "Responses",
                    principalColumn: "ResponseId",
                    onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                    name: "FK_Answer_Questions_QuestionId",
                    column: λ => λ.QuestionId,
                    principalTable: "Questions",
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
