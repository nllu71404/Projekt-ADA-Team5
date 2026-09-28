using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114255_Role")]
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
                    RoleName = table.Column<string>(type: "text", nullable: false) // Bør ændres til RoleEnum senere! :D
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleId", λ => λ.RoleId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
