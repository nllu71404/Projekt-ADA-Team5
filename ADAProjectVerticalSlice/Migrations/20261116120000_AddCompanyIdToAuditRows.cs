using System;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261116120000_AddCompanyIdToAuditRows")]
public sealed class AddCompanyIdToAuditRows : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE audit.row_changes
                ADD COLUMN company_id uuid;

            CREATE FUNCTION audit.set_row_change_company()
            RETURNS trigger
            LANGUAGE plpgsql
            SET search_path = pg_catalog, pg_temp
            AS $$
            BEGIN
                -- Firma-id'et gemmes på auditposten, så det bevares efter sletning.
                NEW.company_id :=
                    NULLIF(current_setting('app.company_id', true), '')::uuid;

                RETURN NEW;
            END;
            $$;

            CREATE TRIGGER set_row_change_company
                BEFORE INSERT ON audit.row_changes
                FOR EACH ROW
                EXECUTE FUNCTION audit.set_row_change_company();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "Use a forward migration to preserve company IDs on audit records.");
    }
}