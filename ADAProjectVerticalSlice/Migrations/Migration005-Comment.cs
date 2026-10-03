using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114251_Comment")]
    public class Migration005_Comment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Theme Comments
            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FreeForm = table.Column<string>(type: "text", nullable: false),
                    RespondentId = table.Column<Guid>(type: null, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", λ => λ.CommentId);

                    table.ForeignKey(
                    name: "FK_Comment_Theme_ThemeId",
                    column: λ => λ.ThemeId,
                    principalTable: "Theme",
                    principalColumn: "ThemeId",
                    onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                    name: "FK_Comment_Respondent_RespondentId",
                    column: λ => λ.RespondentId,
                    principalTable: "Respondent",
                    principalColumn: "RespondentId",
                    onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comment");
        }
    }
}
