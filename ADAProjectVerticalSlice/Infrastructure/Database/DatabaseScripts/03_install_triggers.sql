-- ==============================================
-- SCRIPT 3: INSTALLER AUDIT-TRIGGER PÅ TABEL
-- ==============================================

CREATE OR REPLACE FUNCTION audit.install_on_table(
    p_table REGCLASS
)
RETURNS VOID
LANGUAGE plpgsql
SET search_path = pg_catalog, pg_temp 

AS $$
DECLARE

    -- Tabellens skema.
    v_schema TEXT;

    -- Tabellens navn.
    v_table TEXT;

    -- Argumenter for primærnøglekolonner.
    v_pk_columns TEXT;

BEGIN

    -- ==========================================
    -- 1. FIND EGNET TABEL
    -- ==========================================

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

      -- Almindelige og partitionerede tabeller.
      AND c.relkind IN ('r', 'p')

      -- Udeluk underliggende partitioner.
      AND NOT c.relispartition

      -- Kun tabeller i public-skemaet.
      AND n.nspname = 'public'

      -- Udeluk EF Cores migrationshistorik.
      AND c.relname <> '__EFMigrationsHistory';


    -- Spring tabeller over, der ikke opfylder kravene.
    IF NOT FOUND THEN
        RETURN;
    END IF;


    -- ==========================================
    -- 2. UNDGÅ DOBBELTE TRIGGERE
    -- ==========================================

    IF EXISTS (

        SELECT 1
        FROM pg_trigger

        WHERE tgrelid = p_table
          AND tgname = 'audit_changes'

    ) THEN
        RETURN;
    END IF;


    -- ==========================================
    -- 3. FIND PRIMÆRNØGLEKOLONNER
    -- ==========================================

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


    -- ==========================================
    -- 4. OPRET AUDIT-TRIGGER
    -- ==========================================

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