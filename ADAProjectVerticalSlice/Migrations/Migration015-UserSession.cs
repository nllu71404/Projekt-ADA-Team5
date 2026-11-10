
using System;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114300_UserSession")]
    public class Migration015_UserSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false),

                    Ticket = table.Column<byte[]>(
                        type: "bytea",
                        nullable: false),

                    Created = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false),

                    LastActivity = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false),

                    Expires = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false),

                    Revoked = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true),

                    UserId = table.Column<string>(
                        type: "text",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_UserSessions",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_UserSessions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_Expires",
                table: "UserSessions",
                column: "Expires");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSessions");
        }
    }
}
