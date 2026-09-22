using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919115300_Respondent")]
    public class Migration013_Respondent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Respodent
            migrationBuilder.CreateTable(
                name: "Respodent",
                columns: table => new
                {
                    RespodentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Respodent", λ => λ.RespodentId);

                    table.ForeignKey(
                    name: "FK_Respodent_Assessments_AssessmentId",
                    column: λ => λ.AssessmentId,
                    principalTable: "Assessments",
                    principalColumn: "AssessmentId",
                    onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Respodent");
        }
    }
}
