using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114800_TargetGroupRole")]
    public class Migration008_TargetGroupRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Target Group Role
            migrationBuilder.CreateTable(
                name: "TargetGroupRole",
                columns: table => new
                {
                    TargetGroupRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetGroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetGroupRoleId", λ => λ.TargetGroupRoleId);

                    table.ForeignKey(
                    name: "FK_TargetGroupRole_TargetGroup_TargetGroupId",
                    column: λ => λ.TargetGroupId,
                    principalTable: "TargetGroup",
                    principalColumn: "TargetGroupId",
                    onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TargetGroupRole");
        }
    }
}
