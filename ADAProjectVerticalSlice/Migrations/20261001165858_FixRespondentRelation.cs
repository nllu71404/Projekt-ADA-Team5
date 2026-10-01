using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAProjectAPIVerticalSlice.Migrations
{
    /// <inheritdoc />
    public partial class FixRespondentRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove the existing FK so AssessmentId can be changed
            migrationBuilder.DropForeignKey(
                name: "FK_Respondent_Assessment_AssessmentId",
                table: "Respondent");

            // Make AssessmentId required
            migrationBuilder.AlterColumn<Guid>(
                name: "AssessmentId",
                table: "Respondent",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // Rename database column to match the current C# property
            migrationBuilder.RenameColumn(
                name: "Freeform",
                table: "Comment",
                newName: "FreeForm");

            // Create the index expected by the current EF Core model
            migrationBuilder.CreateIndex(
                name: "IX_Respondent_AssessmentId",
                table: "Respondent",
                column: "AssessmentId");

            // Recreate FK with Cascade delete
            migrationBuilder.AddForeignKey(
                name: "FK_Respondent_Assessment_AssessmentId",
                table: "Respondent",
                column: "AssessmentId",
                principalTable: "Assessment",
                principalColumn: "AssessmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Respondent_Assessment_AssessmentId",
                table: "Respondent");

            migrationBuilder.DropIndex(
                name: "IX_Respondent_AssessmentId",
                table: "Respondent");

            migrationBuilder.RenameColumn(
                name: "FreeForm",
                table: "Comment",
                newName: "Freeform");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssessmentId",
                table: "Respondent",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Respondent_Assessment_AssessmentId",
                table: "Respondent",
                column: "AssessmentId",
                principalTable: "Assessment",
                principalColumn: "AssessmentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}