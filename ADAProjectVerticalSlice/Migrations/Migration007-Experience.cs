using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114700_Experience")]
    public class Migration007_Experience : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Experience
            migrationBuilder.CreateTable(
                name: "Experience",
                columns: table => new
                {
                    ExperienceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetGroupExperienceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExperienceValue = table.Column<string>(type: "RegionEnum", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperienceId", λ => λ.ExperienceId);

                    table.ForeignKey(
                    name: "FK_Experience_TargetGroupExperience_TargetGroupExperienceId",
                    column: λ => λ.TargetGroupExperienceId,
                    principalTable: "TargetGroupExperience",
                    principalColumn: "TargetGroupExperienceId",
                    onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Experience");
        }
    }
}
