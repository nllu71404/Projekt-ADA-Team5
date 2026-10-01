using System;
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
            migrationBuilder.DropColumn(
                name: "Freeform",
                table: "Theme");

            migrationBuilder.DropColumn(
                name: "Experiences",
                table: "Assessment");

            migrationBuilder.CreateTable(
                name: "Experiences",
                columns: table => new
                {
                    ExperienceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExperienceValue = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experiences", x => x.ExperienceId);
                });

            migrationBuilder.CreateTable(
                name: "Respondents",
                columns: table => new
                {
                    RespondentId = table.Column<Guid>(type: "uuid", nullable: false),
                    HasAnswered = table.Column<bool>(type: "boolean", nullable: false),
                    EmailAddress = table.Column<string>(type: "text", nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Respondents", x => x.RespondentId);
                    table.ForeignKey(
                        name: "FK_Respondents_Assessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessment",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentExperience",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExperienceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentExperience", x => new { x.AssessmentId, x.ExperienceId });
                    table.ForeignKey(
                        name: "FK_AssessmentExperience_Assessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessment",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentExperience_Experiences_ExperienceId",
                        column: x => x.ExperienceId,
                        principalTable: "Experiences",
                        principalColumn: "ExperienceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FreeForm = table.Column<string>(type: "text", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RespondentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comment_Respondents_RespondentId",
                        column: x => x.RespondentId,
                        principalTable: "Respondents",
                        principalColumn: "RespondentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comment_Theme_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Theme",
                        principalColumn: "ThemeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentExperience_ExperienceId",
                table: "AssessmentExperience",
                column: "ExperienceId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_RespondentId",
                table: "Comment",
                column: "RespondentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ThemeId",
                table: "Comment",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_ExperienceValue",
                table: "Experiences",
                column: "ExperienceValue",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Respondents_AssessmentId",
                table: "Respondents",
                column: "AssessmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentExperience");

            migrationBuilder.DropTable(
                name: "Comment");

            migrationBuilder.DropTable(
                name: "Experiences");

            migrationBuilder.DropTable(
                name: "Respondents");

            migrationBuilder.AddColumn<string>(
                name: "Freeform",
                table: "Theme",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int[]>(
                name: "Experiences",
                table: "Assessment",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }
    }
}
