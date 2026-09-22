using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114500_TargetGroupRegion")]

    public class Migration005_TargetGroupRegion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Target Group Region
            migrationBuilder.CreateTable(
                name: "TargetGroupRegion",
                columns: table => new
                {
                    TargetGroupRegionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetGroupRegionId", λ => λ.TargetGroupRegionId);

                    table.ForeignKey(
                    name: "FK_TargetGroupRegion_TargetGroup_TargetGroupId",
                    column: λ => λ.TargetGroupId,
                    principalTable: "TargetGroup",
                    principalColumn: "TargetGroupId",
                    onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_TargetGroupRegion_Regions_RegionId",
                        column: λ => λ.RegionId,
                        principalTable: "Regions",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TargetGroupRegion");

            migrationBuilder.DropTable(
                name: "Regions");
        }

    }
}
