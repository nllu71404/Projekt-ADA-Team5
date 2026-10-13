using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAProjectAPIVerticalSlice.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingForeignKeyIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Answer_QuestionId",
                table: "Answer",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Answer_RespondentId",
                table: "Answer",
                column: "RespondentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_RespondentId",
                 table: "Comment",
                column: "RespondentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ThemeId",
                table: "Comment",
                column: "ThemeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Answer_QuestionId",
                table: "Answer");

            migrationBuilder.DropIndex(
                name: "IX_Answer_RespondentId",
                table: "Answer");

            migrationBuilder.DropIndex(
                name: "IX_Comment_RespondentId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_ThemeId",
                table: "Comment");
        }
    }
}
