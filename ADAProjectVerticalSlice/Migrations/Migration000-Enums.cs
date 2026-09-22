using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114000_Enums")]
    public partial class Migration001_Enums : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Application
            // Enums
            migrationBuilder.Sql("""
        CREATE TYPE RegionEnum AS ENUM (
            'United Kingdom East',
            'Continental Europe',
            'United Kingdom West',
            'Americas',
            'Asia Pacific',
            'Middle East',
            'Africa',
            'Nordics',
            'Central Europe',
            'Southern Europe'
        );
        """);

            migrationBuilder.Sql("""
        CREATE TYPE ExperienceEnum AS ENUM (
            '<1',
            '1-3',
            '3-5',
            '5+'
        );
        """);

            migrationBuilder.Sql("""
        CREATE TYPE RoleEnum AS ENUM (
            'employee',
            'manager',
            'director',
            'executive'
        );
        """);


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TYPE RegionEnum");

            migrationBuilder.Sql("DROP TYPE ExperienceEnum");

            migrationBuilder.Sql("DROP TYPE RoleEnum");
        }
    }
}