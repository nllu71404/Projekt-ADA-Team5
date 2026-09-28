using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114252_Question")]
    public class Migration006_Question : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Questions
            migrationBuilder.CreateTable(
                name: "Question",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<string>(type: "text", nullable: false),
                    QuestionPolarity = table.Column<string>(type: "text", nullable: false),
                    SurveyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Question", λ => λ.QuestionId);

                    table.ForeignKey(
                        name: "FK_Question_Theme_ThemeId",
                        column: λ => λ.ThemeId,
                        principalTable: "Theme",
                        principalColumn: "ThemeId",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_Question_Survey_SurveyId",
                        column: λ => λ.SurveyId,
                        principalTable: "Survey",
                        principalColumn: "SurveyId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Question");
        }
    }
}
