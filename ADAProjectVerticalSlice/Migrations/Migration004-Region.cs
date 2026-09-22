using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114400_Region")]
    public class Migration004_Region : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Region
            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    RegionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionName = table.Column<string>(type: "text", nullable: false) // Bør ændres til RegionEnum senere! :D
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionId", λ => λ.RegionId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Regions");
        }
    }
}
