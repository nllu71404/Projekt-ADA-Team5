using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [Migration("20260919114200_Survey")]
    public partial class Migration002_Survey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Survey",
                columns: table => new
                {
                    SurveyId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false),

                    Title = table.Column<string>(
                        type: "text",
                        nullable: false),

                    Description = table.Column<string>(
                        type: "text",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_Survey",
                        x => x.SurveyId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Survey");
        }
    }
}
