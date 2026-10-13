using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Net.Mail;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114152_Respondent")]
    public class Migration0111_Respondent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Respondent",
                columns: table => new
                {
                    RespondentId = table.Column<Guid>(type: "uuid", nullable: false),
                    HasAnswered = table.Column<bool>(type: "boolean", nullable: false),
                    EmailAddress = table.Column<string>(type: "text", nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExperienceId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true)
                },

                constraints: table =>
                {
                    table.PrimaryKey("PK_Respondent", λ => λ.RespondentId);

                    table.ForeignKey(
                        name: "FK_Respondent_Assessment_AssessmentId",
                        column: λ => λ.AssessmentId,
                        principalTable: "Assessment",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_Respondent_Regions_RegionId",
                        column: λ => λ.RegionId,
                        principalTable: "Regions",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_Respondent_Experiences_ExperienceId",
                        column: λ => λ.ExperienceId,
                        principalTable: "Experiences",
                        principalColumn: "ExperienceId",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_Respondent_Roles_RoleId",
                        column: λ => λ.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Respondent");
        }
    }
}

