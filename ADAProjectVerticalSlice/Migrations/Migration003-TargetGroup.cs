using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114300_TargetGroup")]
    public class Migration003_TargetGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Target Group
            migrationBuilder.CreateTable(
                name: "TargetGroup",
                columns: table => new
                {
                    TargetGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    SurveyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetGroupId", λ => λ.TargetGroupId);

                    table.ForeignKey(
                    name: "FK_TargetGroup_Survey_SurveyId",
                    column: λ => λ.SurveyId,
                    principalTable: "Survey",
                    principalColumn: "SurveyId",
                    onDelete: ReferentialAction.Cascade);
                });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TargetGroup");
        }
    }
}
