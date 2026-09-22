using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace ADAProjectAPIVerticalSlice.Migrations
{
    public partial class AddAssessmentRolesAndRegions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.AddColumn<int[]>(
                name: "Experiences",
                table: "Assessments",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    RegionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.RegionId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });
            
            migrationBuilder.CreateTable(
                name: "AssessmentRegion",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionsRegionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentRegion", x => new { x.AssessmentId, x.RegionsRegionId });
                    table.ForeignKey(
                        name: "FK_AssessmentRegion_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentRegion_Regions_RegionsRegionId",
                        column: x => x.RegionsRegionId,
                        principalTable: "Regions",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Cascade);
                });


            
            migrationBuilder.CreateTable(
                name: "AssessmentRole",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RolesRoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentRole", x => new { x.AssessmentId, x.RolesRoleId });
                    table.ForeignKey(
                        name: "FK_AssessmentRole_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentRole_Roles_RolesRoleId",
                        column: x => x.RolesRoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });
            */

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentRegion_RegionsRegionId",
                table: "AssessmentRegion",
                column: "RegionsRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentRole_RolesRoleId",
                table: "AssessmentRole",
                column: "RolesRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_RegionName",
                table: "Regions",
                column: "RegionName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.DropTable(
                name: "AssessmentRegion");

            migrationBuilder.DropTable(
                name: "AssessmentRole");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "Roles");
            
            migrationBuilder.DropColumn(
                name: "Experiences",
                table: "Assessments");
            */
        }
    }
}
