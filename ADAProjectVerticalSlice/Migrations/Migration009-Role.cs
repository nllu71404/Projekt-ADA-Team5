using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114900_Role")]
    public class Migration009_Role : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Role
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetGroupRoleId = table.Column<Guid>(type: "uuid", nullable: true), // Skal ændres til false senere! :D
                    RoleName = table.Column<string>(type: "text", nullable: false) // Bør ændres til RoleEnum senere! :D
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleId", λ => λ.RoleId);

                    table.ForeignKey(
                    name: "FK_Roles_TargetGroupRole_TargetGroupRoleId",
                    column: λ => λ.TargetGroupRoleId,
                    principalTable: "TargetGroupRole",
                    principalColumn: "TargetGroupRoleId",
                    onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
