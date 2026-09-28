using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114259_AssessmentRole")]
    public class Migration010_AssessmentRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
            name: "AssessmentRole",
            columns: table => new
            {
                AssessmentId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),

                RoleId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_AssessmentRole",
                    x => new
                    {
                        x.AssessmentId,
                        x.RoleId
                    });

        table.ForeignKey(
            name: "FK_AssessmentRole_Assessment_AssessmentId",
            column: x => x.AssessmentId,
            principalTable: "Assessment",
            principalColumn: "AssessmentId",
            onDelete: ReferentialAction.Cascade);

        table.ForeignKey(
            name: "FK_AssessmentRole_Roles_RoleId",
            column: x => x.RoleId,
            principalTable: "Roles",
            principalColumn: "RoleId",
            onDelete: ReferentialAction.Cascade);
         });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentRole_RoleId",
                table: "AssessmentRole",
                column: "RoleId");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentRole");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
