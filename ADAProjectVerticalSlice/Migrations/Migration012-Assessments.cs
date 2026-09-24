using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919115200_Assessments")]
    public class Migration012_Assessments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false),

                    AssessmentName = table.Column<string>(
                        type: "text",
                        nullable: false),

                    StartDate = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false),

                    EndDate = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false),

                    UserId = table.Column<string>(
                        type: "text",
                        nullable: false),

                    ApplicationId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false),

                    SurveyId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false),

                    Experiences = table.Column<int[]>(
                        type: "integer[]",
                        nullable: false,
                        defaultValueSql: "ARRAY[]::integer[]")
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_Assessments",
                        x => x.AssessmentId);

                    table.ForeignKey(
                        name: "FK_Assessments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_Assessments_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_Assessments_Survey_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Survey",
                        principalColumn: "SurveyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_UserId",
                table: "Assessments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_ApplicationId",
                table: "Assessments",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_SurveyId",
                table: "Assessments",
                column: "SurveyId");


            // Assessment <-> Role
            migrationBuilder.CreateTable(
                name: "AssessmentRole",
                columns: table => new
                {
                    AssessmentsAssessmentId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false),

                    RolesRoleId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_AssessmentRole",
                        x => new
                        {
                            x.AssessmentsAssessmentId,
                            x.RolesRoleId
                        });

                    table.ForeignKey(
                        name: "FK_AssessmentRole_Assessments_AssessmentsAssessmentId",
                        column: x => x.AssessmentsAssessmentId,
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

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentRole_RolesRoleId",
                table: "AssessmentRole",
                column: "RolesRoleId");


            // Assessment <-> Region
            migrationBuilder.CreateTable(
                name: "AssessmentRegion",
                columns: table => new
                {
                    AssessmentsAssessmentId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false),

                    RegionsRegionId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_AssessmentRegion",
                        x => new
                        {
                            x.AssessmentsAssessmentId,
                            x.RegionsRegionId
                        });

                    table.ForeignKey(
                        name: "FK_AssessmentRegion_Assessments_AssessmentsAssessmentId",
                        column: x => x.AssessmentsAssessmentId,
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

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentRegion_RegionsRegionId",
                table: "AssessmentRegion",
                column: "RegionsRegionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentRole");

            migrationBuilder.DropTable(
                name: "AssessmentRegion");

            migrationBuilder.DropTable(
                name: "Assessments");
        }
    }
}

