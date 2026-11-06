-- ==========================================
-- SCRIPT 04: AUTOMATISK INSTALLATION AF AUDIT-TRIGGERE
-- ==========================================

-- 1. Opret funktion til event-triggeren.
CREATE OR REPLACE FUNCTION audit.on_create_table()
RETURNS EVENT_TRIGGER
LANGUAGE plpgsql
SET search_path = pg_catalog, pg_temp
AS $$
DECLARE
    cmd RECORD;

BEGIN

    FOR cmd IN

        SELECT DISTINCT objid
        FROM pg_event_trigger_ddl_commands()
        WHERE command_tag IN (
            'CREATE TABLE',
            'CREATE TABLE AS'
        )
        AND classid = 'pg_catalog.pg_class'::regclass
        AND NOT in_extension

    LOOP

        PERFORM audit.install_on_table(
            cmd.objid::regclass
        );

    END LOOP;

END;
$$;


-- 2. Opret event-trigger til automatisk installation.
DROP EVENT TRIGGER IF EXISTS audit_new_tables;

CREATE EVENT TRIGGER audit_new_tables
ON ddl_command_end
WHEN TAG IN (
    'CREATE TABLE',
    'CREATE TABLE AS'
)
EXECUTE FUNCTION audit.on_create_table();


-- 3. Installer audit-triggere på eksisterende tabeller.
DO $$
DECLARE
    v_table RECORD;

BEGIN

    FOR v_table IN

        SELECT c.oid
        FROM pg_class c
        JOIN pg_namespace n
            ON n.oid = c.relnamespace
        WHERE n.nspname = 'public'
          AND c.relkind IN ('r', 'p')
          AND NOT c.relispartition
          AND c.relname <> '__EFMigrationsHistory'

    LOOP

        PERFORM audit.install_on_table(
            v_table.oid::regclass
        );

    END LOOP;

END;
$$;