using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAProjectAPIVerticalSlice.Migrations
{
    /// <inheritdoc />
    public partial class CheckModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Surveys_SurveyId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Surveys_SurveyId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Themes_Surveys_SurveyId",
                table: "Themes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Surveys",
                table: "Surveys");

            migrationBuilder.RenameTable(
                name: "Surveys",
                newName: "Survey");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Survey",
                table: "Survey",
                column: "SurveyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Survey_SurveyId",
                table: "Assessments",
                column: "SurveyId",
                principalTable: "Survey",
                principalColumn: "SurveyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Survey_SurveyId",
                table: "Questions",
                column: "SurveyId",
                principalTable: "Survey",
                principalColumn: "SurveyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Themes_Survey_SurveyId",
                table: "Themes",
                column: "SurveyId",
                principalTable: "Survey",
                principalColumn: "SurveyId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Survey_SurveyId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Survey_SurveyId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Themes_Survey_SurveyId",
                table: "Themes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Survey",
                table: "Survey");

            migrationBuilder.RenameTable(
                name: "Survey",
                newName: "Surveys");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Surveys",
                table: "Surveys",
                column: "SurveyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Surveys_SurveyId",
                table: "Assessments",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "SurveyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Surveys_SurveyId",
                table: "Questions",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "SurveyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Themes_Surveys_SurveyId",
                table: "Themes",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "SurveyId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
