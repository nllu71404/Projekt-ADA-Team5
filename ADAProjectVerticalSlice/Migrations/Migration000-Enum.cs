using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919114000_Enum")]
    public partial class Migration000_Enum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Application
            // Enums
            migrationBuilder.Sql("""
        CREATE TYPE Region AS ENUM (
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
        CREATE TYPE Experience AS ENUM (
            'LessThan1Year',
            'From1To2Years',
            'From3To5Years',
            'MoreThan5Years' 
        );
        """);

            migrationBuilder.Sql("""
        CREATE TYPE Role AS ENUM (
            'employee',
            'manager',
            'director',
            'executive'
        );
        """);


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TYPE Region");

            migrationBuilder.Sql("DROP TYPE Experience");

            migrationBuilder.Sql("DROP TYPE Role");
        }
    }
}