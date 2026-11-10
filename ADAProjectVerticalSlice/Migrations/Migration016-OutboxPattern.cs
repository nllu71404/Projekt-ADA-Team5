using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114900_OutboxPattern")]
    public partial class OutboxPattern : Migration
        {
            /// <inheritdoc />
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable(
                    name: "OutboxMessages",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "uuid", nullable: false),
                        Type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                        RoutingKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                        Payload = table.Column<string>(type: "text", nullable: false),
                        CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                        ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                        RetryCount = table.Column<int>(type: "integer", nullable: false),
                        LastError = table.Column<string>(type: "text", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                    });

                migrationBuilder.CreateIndex(
                    name: "IX_OutboxMessages_ProcessedOnUtc_CreatedOnUtc",
                    table: "OutboxMessages",
                    columns: new[] { "ProcessedOnUtc", "CreatedOnUtc" });
            }

            /// <inheritdoc />
            protected override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.DropTable(
                    name: "OutboxMessages");
            }
        }
    }
