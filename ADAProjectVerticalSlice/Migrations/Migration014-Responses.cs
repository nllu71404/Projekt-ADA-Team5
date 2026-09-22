using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919115400_Responses")]

    public class Migration014_Responses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Repsonses
            migrationBuilder.CreateTable(
                name: "Responses",
                columns: table => new
                {
                    ResponseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SurveyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RespodentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responses", λ => λ.ResponseId);

                    table.ForeignKey(
                    name: "FK_Responses_Survey_SurveyId",
                    column: λ => λ.SurveyId,
                    principalTable: "Survey",
                    principalColumn: "SurveyId",
                    onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                    name: "FK_Responses_Respodent_RespodentId",
                    column: λ => λ.RespodentId,
                    principalTable: "Respodent",
                    principalColumn: "RespodentId",
                    onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Responses");
        }
    }
}
