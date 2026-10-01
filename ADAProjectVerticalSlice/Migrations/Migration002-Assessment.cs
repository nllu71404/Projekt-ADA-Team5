using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114150_Assessment")]
    public class Migration002_Assessment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assessment",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentName = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SurveyId = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_Assessment",
                        x => x.AssessmentId);

                    table.ForeignKey(
                        name: "FK_Assessment_Application_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Application",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_Assessment_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_Assessment_Survey_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Survey",
                        principalColumn: "SurveyId",
                        onDelete: ReferentialAction.Restrict);
                });
            /*
            migrationBuilder.CreateIndex(
                name: "IX_Assessment_ApplicationId",
                table: "Assessment",
                column: "ApplicationId");*/
            /*
            migrationBuilder.CreateIndex(
                name: "IX_Assessment_SurveyId",
                table: "Assessment",
                column: "SurveyId");

            
            // Assessment <-> Role
            migrationBuilder.CreateTable(
                name: "AssessmentRole",
                columns: table => new
                {
                    AssessmentAssessmentId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false),

                    RoleRoleId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_AssessmentRole",
                        x => new
                        {
                            x.AssessmentAssessmentId,
                            x.RoleRoleId
                        });

                    table.ForeignKey(
                        name: "FK_AssessmentRole_Assessment_AssessmentAssessmentId",
                        column: x => x.AssessmentAssessmentId,
                        principalTable: "Assessment",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_AssessmentRole_Role_RoleRoleId",
                        column: x => x.RoleRoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentRole_RoleRoleId",
                table: "AssessmentRole",
                column: "RoleRoleId");


            // Assessment <-> Region
            migrationBuilder.CreateTable(
                name: "AssessmentRegion",
                columns: table => new
                {
                    AssessmentAssessmentId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false),

                    RegionRegionId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_AssessmentRegion",
                        x => new
                        {
                            x.AssessmentAssessmentId,
                            x.RegionRegionId
                        });

                    table.ForeignKey(
                        name: "FK_AssessmentRegion_Assessment_AssessmentAssessmentId",
                        column: x => x.AssessmentAssessmentId,
                        principalTable: "Assessment",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_AssessmentRegion_Region_RegionRegionId",
                        column: x => x.RegionRegionId,
                        principalTable: "Region",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentRegion_RegionRegionId",
                table: "AssessmentRegion",
                column: "RegionRegionId");


        // Assessment <-> Region
        migrationBuilder.CreateTable(
            name: "AssessmentExperience",
                columns: table => new
                {
                    AssessmentAssessmentId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false),

                    ExperienceExperienceId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_AssessmentExperience",
                        x => new
                        {
                            x.AssessmentAssessmentId,
                            x.ExperienceExperienceId
                        });

                    table.ForeignKey(
                        name: "FK_AssessmentExperience_Assessment_AssessmentAssessmentId",
                        column: x => x.AssessmentAssessmentId,
                        principalTable: "Assessment",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_AssessmentExperience_Experience_ExperienceExperienceId",
                        column: x => x.ExperienceExperienceId,
                        principalTable: "Experience",
                        principalColumn: "ExperienceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentExperience_ExperienceExperienceId",
                table: "AssessmentExperience",
                column: "ExperienceExperienceId"); */
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assessment");
        }
    }
}

