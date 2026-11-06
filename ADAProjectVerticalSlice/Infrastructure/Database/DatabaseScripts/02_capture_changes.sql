-- ==============================================
-- SCRIPT 2: FUNKTION TIL REGISTRERING AF ÆNDRINGER
-- ==============================================

CREATE OR REPLACE FUNCTION audit.capture_change()
RETURNS TRIGGER
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, pg_temp 

AS $$
DECLARE

    -- Tidligere rækkedata.
    v_old JSONB;

    -- Nye rækkedata.
    v_new JSONB;

    -- Række, der bruges til at hente primærnøgleværdier.
    v_row JSONB;

    -- JSON-objekt med primærnøgleværdier.
    v_pk JSONB := '{}'::jsonb;

    -- Løkketæller.
    i INTEGER;

BEGIN

    -- ==========================================
    -- 1. IDENTIFICER OPERATION
    -- ==========================================

    IF TG_OP = 'INSERT' THEN

        v_new := to_jsonb(NEW);

    ELSIF TG_OP = 'UPDATE' THEN

        v_old := to_jsonb(OLD);
        v_new := to_jsonb(NEW);

        -- Spring opdateringer over, hvor ingen værdier er ændret.
        IF v_old = v_new THEN
            RETURN NULL;
        END IF;

    ELSIF TG_OP = 'DELETE' THEN

        v_old := to_jsonb(OLD);

    END IF;


    -- ==========================================
    -- 2. HENT PRIMÆRNØGLEVÆRDIER
    -- ==========================================

    -- INSERT / UPDATE bruger NEW.
    -- DELETE bruger OLD.
    v_row := COALESCE(v_new, v_old);

    -- TG_ARGV indeholder navnene på primærnøglekolonnerne.
    FOR i IN 0..TG_NARGS - 1 LOOP

        v_pk := v_pk || jsonb_build_object(
            TG_ARGV[i],
            v_row -> TG_ARGV[i]
        );

    END LOOP;

    -- Brug NULL, hvis tabellen ikke har en primærnøgle.
    IF v_pk = '{}'::jsonb THEN
        v_pk := NULL;
    END IF;


    -- ==========================================
    -- 3. FJERN FØLSOMME DATA
    -- ==========================================

    -- Fjern følsomme felter fra tidligere rækkedata.
    v_old := v_old - ARRAY[
        'password_hash',
        'access_token',
        'refresh_token',
        'secret'
    ];

    -- Fjern følsomme felter fra nye rækkedata.
    v_new := v_new - ARRAY[
        'password_hash',
        'access_token',
        'refresh_token',
        'secret'
    ];


    -- ==========================================
    -- 4. INDSÆT POST I ÆNDRINGSLOGGEN
    -- ==========================================

    INSERT INTO audit.row_changes (

        schema_name,
        table_name,
        operation,
        row_pk,
        actor_id,
        request_id,
        database_user,
        old_data,
        new_data

    )

    VALUES (

        -- Kildeskema.
        TG_TABLE_SCHEMA,

        -- Kildetabel.
        TG_TABLE_NAME,

        -- INSERT / UPDATE / DELETE.
        TG_OP,

        -- Primærnøgle.
        v_pk,

        -- Applikationsbrugerens ID.
        NULLIF(
            current_setting('app.actor_id', true),
            ''
        ),

        -- Applikationens forespørgsels-ID.
        NULLIF(
            current_setting('app.request_id', true),
            ''
        ),

        -- Oprindelig databasesessionsbruger.
        session_user,

        -- Tidligere række.
        v_old,

        -- Ny række.
        v_new
    );


    -- AFTER-triggere ignorerer returværdien.
    RETURN NULL;

END;
$$;