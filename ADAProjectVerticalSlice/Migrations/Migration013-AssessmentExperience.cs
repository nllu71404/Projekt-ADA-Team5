using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114257_AssessmentExperience")]
    public class Migration010_AssessmentExperience : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
            name: "AssessmentExperience",
            columns: table => new
            {
                AssessmentId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),

                ExperienceId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_AssessmentExperience",
                    x => new
                    {
                        x.AssessmentId,
                        x.ExperienceId
            });

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

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentExperience_ExperienceId",
                table: "AssessmentExperience",
                column: "ExperienceId");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentExperience");

            migrationBuilder.DropTable(
                name: "Experiences");
        }
    }
}
