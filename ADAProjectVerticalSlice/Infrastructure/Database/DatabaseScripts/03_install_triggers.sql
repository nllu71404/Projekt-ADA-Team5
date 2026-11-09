-- ==========================================
-- SCRIPT 03: INSTALLER AUDIT-TRIGGERE
-- ==========================================

CREATE OR REPLACE FUNCTION audit.install_on_table(
    p_table REGCLASS
)
RETURNS VOID
LANGUAGE plpgsql
SET search_path = pg_catalog, pg_temp
AS $$
DECLARE
    v_schema TEXT;
    v_table TEXT;
    v_pk_columns TEXT;

BEGIN

    -- Find en tabel, der opfylder kravene.
    SELECT
        n.nspname,
        c.relname
    INTO
        v_schema,
        v_table
    FROM pg_class c
    JOIN pg_namespace n
        ON n.oid = c.relnamespace
    WHERE c.oid = p_table
      AND c.relkind IN ('r', 'p')
      AND NOT c.relispartition
      AND n.nspname = 'public'
      AND c.relname <> '__EFMigrationsHistory';

    IF NOT FOUND THEN
        RETURN;
    END IF;

    -- Kontrollér, om triggeren allerede findes.
    IF EXISTS (
        SELECT 1
        FROM pg_trigger
        WHERE tgrelid = p_table
          AND tgname = 'audit_changes'
    ) THEN
        RETURN;
    END IF;

    -- Find kolonnerne, der udgør primærnøglen.
    SELECT COALESCE(
        string_agg(
            format('%L', a.attname),
            ', ' ORDER BY k.ordinality
        ),
        ''
    )
    INTO v_pk_columns
    FROM pg_index ix
    CROSS JOIN LATERAL unnest(ix.indkey)
        WITH ORDINALITY AS k(attnum, ordinality)
    JOIN pg_attribute a
        ON a.attrelid = ix.indrelid
       AND a.attnum = k.attnum
    WHERE ix.indrelid = p_table
      AND ix.indisprimary
      AND k.ordinality <= ix.indnkeyatts;

    -- Opret audit-triggeren.
    EXECUTE format(
        'CREATE TRIGGER audit_changes
         AFTER INSERT OR UPDATE OR DELETE
         ON %I.%I
         FOR EACH ROW
         EXECUTE FUNCTION audit.capture_change(%s)',
        v_schema,
        v_table,
        v_pk_columns
    );

END;
$$;