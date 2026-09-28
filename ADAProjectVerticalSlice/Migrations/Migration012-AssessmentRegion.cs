using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114258_AssessmentRegion")]
    public class Migration010_AssessmentRegion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
            name: "AssessmentRegion",
            columns: table => new
            {
                AssessmentId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),

                RegionId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_AssessmentRegion",
                    x => new
                    {
                        x.AssessmentId,
                        x.RegionId
            });

        table.ForeignKey(
            name: "FK_AssessmentRegion_Assessment_AssessmentId",
            column: x => x.AssessmentId,
            principalTable: "Assessment",
            principalColumn: "AssessmentId",
            onDelete: ReferentialAction.Cascade);

        table.ForeignKey(
            name: "FK_AssessmentRegion_Regions_RegionId",
            column: x => x.RegionId,
            principalTable: "Regions",
            principalColumn: "RegionId",
            onDelete: ReferentialAction.Cascade);
         });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentRegion_RegionId",
                table: "AssessmentRegion",
                column: "RegionId");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentRegion");

            migrationBuilder.DropTable(
                name: "Regions");
        }
    }
}
