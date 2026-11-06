using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919115000_Auditlog")]
    public class Migration020_Auditlog : Migration
    {
            protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create audit schema, table and indexes.
            migrationBuilder.Sql(
                ReadSql("01_create_audit_table.sql"));

            // 2. Create audit capture function.
            migrationBuilder.Sql(
                ReadSql("02_capture_changes.sql"));

            // 3. Create trigger installation function.
            migrationBuilder.Sql(
                ReadSql("03_install_triggers.sql"));

            // 4. Create automatic event trigger and
            // install audit triggers on existing tables.
            migrationBuilder.Sql(
                ReadSql("04_auto_install.sql"));
        }

        protected override void Down(
            MigrationBuilder migrationBuilder)
        {
            // Remove automatic event trigger.
            migrationBuilder.Sql("""
                DROP EVENT TRIGGER IF EXISTS audit_new_tables;
                """);

            // Remove row-level audit triggers.
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    r RECORD;
                BEGIN
                    FOR r IN
                        SELECT n.nspname, c.relname
                        FROM pg_class c
                        JOIN pg_namespace n
                            ON n.oid = c.relnamespace
                        JOIN pg_trigger t
                            ON t.tgrelid = c.oid
                        WHERE n.nspname = 'public'
                          AND c.relkind IN ('r', 'p')
                          AND NOT c.relispartition
                          AND t.tgname = 'audit_changes'
                          AND NOT t.tgisinternal
                    LOOP
                        EXECUTE format(
                            'DROP TRIGGER IF EXISTS audit_changes ON %I.%I',
                            r.nspname,
                            r.relname
                        );
                    END LOOP;
                END;
                $$;
                """);

            // Remove auditing functions.
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS audit.on_create_table();
                DROP FUNCTION IF EXISTS audit.install_on_table(regclass);
                DROP FUNCTION IF EXISTS audit.capture_change();
                """);

            // Intentionally preserve audit.row_changes
            // and its historical records.
        }

        private static string ReadSql(string fileName)
        {
            var assembly = typeof(Migration020_Auditlog).Assembly;

            var resourceName = $"AuditSql.{fileName}";

            using var stream =
                assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                var availableResources =
                    assembly.GetManifestResourceNames();

                throw new InvalidOperationException(
                    $"SQL resource not found: {resourceName}\n" +
                    $"Available resources:\n" +
                    string.Join("\n", availableResources)
                );
            }

            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
