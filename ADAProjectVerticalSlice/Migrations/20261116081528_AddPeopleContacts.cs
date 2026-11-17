using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAProjectAPIVerticalSlice.Migrations
{
    /// <inheritdoc />
    public partial class AddPeopleContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeopleContacts",
                columns: table => new
                {
                    PeopleContactId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    FullName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    JobTitle = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Department = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Role = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Region = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Office = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    YearsOfExperience = table.Column<decimal>(type: "numeric", nullable: true),
                    ExperienceLevel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeopleContacts", x => x.PeopleContactId);
                    table.ForeignKey(
                        name: "FK_PeopleContacts_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_PeopleContacts_CompanyId_NormalizedEmail",
                table: "PeopleContacts",
                columns: new[] { "CompanyId", "NormalizedEmail" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeopleContacts");
        }
    }
}
