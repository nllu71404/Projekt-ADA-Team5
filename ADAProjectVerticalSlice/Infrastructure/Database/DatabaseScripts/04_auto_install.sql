-- ==============================================
-- SCRIPT 4: AUTOMATISK INSTALLATION AF AUDIT
-- ==============================================


-- ==========================================
-- 1. OPRET FUNKTION TIL EVENT-TRIGGER
-- ==========================================

CREATE OR REPLACE FUNCTION audit.on_create_table()
RETURNS EVENT_TRIGGER
LANGUAGE plpgsql
SET search_path = pg_catalog, pg_temp

AS $$
DECLARE

    -- Nyligt oprettet databaseobjekt.
    cmd RECORD;

BEGIN

    -- Find tabeller oprettet af DDL-kommandoen.
    FOR cmd IN

        SELECT DISTINCT d.objid

        FROM pg_event_trigger_ddl_commands() AS d

        WHERE d.command_tag IN (
            'CREATE TABLE',
            'CREATE TABLE AS'
        )

          -- Kun objekter fra pg_class.
          AND d.classid =
              'pg_catalog.pg_class'::regclass

          -- Udeluk objekter fra udvidelser.
          AND NOT d.in_extension

    LOOP

        -- Installer audit-trigger på den nye tabel.
        PERFORM audit.install_on_table(
            cmd.objid::regclass
        );

    END LOOP;

END;
$$;


-- ==========================================
-- 2. OPRET POSTGRESQL EVENT-TRIGGER
-- ==========================================

-- Fjern eksisterende event-trigger, hvis den findes.
-- Dette gør det muligt at gentage installationen.
DROP EVENT TRIGGER IF EXISTS audit_new_tables;


-- Registrer automatisk nyoprettede tabeller.
CREATE EVENT TRIGGER audit_new_tables

ON ddl_command_end

WHEN TAG IN (
    'CREATE TABLE',
    'CREATE TABLE AS'
)

EXECUTE FUNCTION audit.on_create_table();


-- Sørg for, at event-triggeren er aktiveret.
ALTER EVENT TRIGGER audit_new_tables ENABLE;


-- ==========================================
-- 3. INSTALLER PÅ EKSISTERENDE TABELLER
-- ==========================================

-- Registrering af nye tabeller påvirker ikke tabeller,
-- der allerede eksisterede før installationen.

DO $$
DECLARE

    v_table RECORD;

BEGIN

    -- Find eksisterende tabeller, der opfylder kravene.
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

        -- Installer triggeren, hvis den mangler.
        PERFORM audit.install_on_table(
            v_table.oid::regclass
        );

    END LOOP;

END;
$$;