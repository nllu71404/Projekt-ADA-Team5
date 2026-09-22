using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114600_TargetGroupExperience")]
    public class Migration006_TargetGroupExperience : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Target Group Experience
            migrationBuilder.CreateTable(
                name: "TargetGroupExperience",
                columns: table => new
                {
                    TargetGroupExperienceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetGroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetGroupExperienceId", λ => λ.TargetGroupExperienceId);

                    table.ForeignKey(
                    name: "FK_TargetGroupExperience_TargetGroup_TargetGroupId",
                    column: λ => λ.TargetGroupId,
                    principalTable: "TargetGroup",
                    principalColumn: "TargetGroupId",
                    onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TargetGroupExperience");
        }
    }
}
