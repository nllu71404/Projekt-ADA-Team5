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
    [Migration("20260919114151_Respondent")]
    public class Migration0031_Respondent : Migration
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
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true)
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
                });
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Respondent");
        }
    }
}

