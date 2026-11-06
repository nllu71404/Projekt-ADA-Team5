-- ==============================================
-- SCRIPT 1: AUDIT-SKEMA OG TABEL
-- ==============================================

-- Opret audit-skemaet.
CREATE SCHEMA IF NOT EXISTS audit;


-- Opret tabel til ændringslog.
CREATE TABLE IF NOT EXISTS audit.row_changes (

    -- Unikt ID for logposten.
    id BIGINT GENERATED ALWAYS AS IDENTITY
        PRIMARY KEY,

    -- Tidspunkt for ændringen.
    changed_at TIMESTAMPTZ NOT NULL
        DEFAULT clock_timestamp(),

    -- Oplysninger om tabellen.
    schema_name TEXT NOT NULL,
    table_name TEXT NOT NULL,

    -- INSERT, UPDATE eller DELETE.
    operation TEXT NOT NULL,

    -- Primærnøgle for den berørte række.
    row_pk JSONB,

    -- Oplysninger om applikationsbruger og forespørgsel.
    actor_id TEXT,
    request_id TEXT,

    -- PostgreSQL-sessionsbruger.
    database_user TEXT NOT NULL,

    -- Rækkedata før og efter ændringen.
    old_data JSONB,
    new_data JSONB
);


-- Indeks til søgning efter ændringer baseret på tabel og tidspunkt.
CREATE INDEX IF NOT EXISTS ix_audit_table_time
ON audit.row_changes (
    schema_name,
    table_name,
    changed_at DESC
);


-- Indeks til søgning efter ændringer baseret på bruger.
CREATE INDEX IF NOT EXISTS ix_audit_actor
ON audit.row_changes (
    actor_id,
    changed_at DESC
);


-- Indeks til søgning efter primærnøgler i JSONB-format.
CREATE INDEX IF NOT EXISTS ix_audit_pk
ON audit.row_changes
USING GIN (row_pk jsonb_path_ops);