using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919115100_Questions")]
    public class Migration011_Questions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Questions
            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SurveyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<string>(type: "text", nullable: false),
                    QuestionPolarity = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", λ => λ.QuestionId);

                    table.ForeignKey(
                    name: "FK_Questions_Survey_SurveyId",
                    column: λ => λ.SurveyId,
                    principalTable: "Survey",
                    principalColumn: "SurveyId",
                    onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                    name: "FK_Questions_Themes_ThemeId",
                    column: λ => λ.ThemeId,
                    principalTable: "Themes",
                    principalColumn: "ThemeId",
                    onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Questions");
        }
    }
}
