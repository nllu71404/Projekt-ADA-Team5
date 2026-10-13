using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ADAProjectAPIVerticalSlice.Entities;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114151_Region")]
    public class Migration010_Region : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Region
            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    RegionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegionName = table.Column<string>(type: "text", nullable: true)
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

