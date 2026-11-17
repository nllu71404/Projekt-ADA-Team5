using System;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADAProjectAPIVerticalSlice.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261114120000_NarrowAssessmentAuditScope")]
public sealed class NarrowAssessmentAuditScope : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP EVENT TRIGGER IF EXISTS audit_new_tables;

            DO $$
            DECLARE
                audit_trigger record;
            BEGIN
                FOR audit_trigger IN
                    SELECT n.nspname AS schema_name,
                           c.relname AS table_name,
                           t.tgname AS trigger_name
                    FROM pg_trigger AS t
                    JOIN pg_class AS c ON c.oid = t.tgrelid
                    JOIN pg_namespace AS n ON n.oid = c.relnamespace
                    WHERE NOT t.tgisinternal
                      AND t.tgparentid = 0
                      AND t.tgfoid = 'audit.capture_change()'::regprocedure
                      AND (
                          n.nspname <> 'public'
                          OR c.relname NOT IN (
                              'Assessment',
                              'AssessmentRole',
                              'AssessmentRegion',
                              'AssessmentExperience'
                          )
                      )
                LOOP
                    EXECUTE format(
                        'DROP TRIGGER %I ON %I.%I',
                        audit_trigger.trigger_name,
                        audit_trigger.schema_name,
                        audit_trigger.table_name
                    );
                END LOOP;
            END;
            $$;

            CREATE OR REPLACE FUNCTION audit.capture_change()
            RETURNS trigger
            LANGUAGE plpgsql
            SECURITY DEFINER
            SET search_path = pg_catalog, pg_temp
            AS $$
            DECLARE
                allowed_fields text[];
                old_values jsonb;
                new_values jsonb;
                key_values jsonb;
                row_key jsonb := '{}'::jsonb;
                key_column text;
            BEGIN
                IF TG_TABLE_SCHEMA <> 'public' THEN
                    RETURN NULL;
                END IF;

                allowed_fields := CASE TG_TABLE_NAME
                    WHEN 'Assessment' THEN
                        ARRAY['AssessmentName', 'StartDate', 'EndDate']
                    WHEN 'AssessmentRole' THEN
                        ARRAY['AssessmentId', 'RoleId']
                    WHEN 'AssessmentRegion' THEN
                        ARRAY['AssessmentId', 'RegionId']
                    WHEN 'AssessmentExperience' THEN
                        ARRAY['AssessmentId', 'ExperienceId']
                END;

                IF allowed_fields IS NULL THEN
                    RETURN NULL;
                END IF;

                IF TG_OP IN ('UPDATE', 'DELETE') THEN
                    SELECT jsonb_object_agg(field.key, field.value)
                    INTO old_values
                    FROM jsonb_each(to_jsonb(OLD)) AS field
                    WHERE field.key = ANY (allowed_fields);
                END IF;

                IF TG_OP IN ('INSERT', 'UPDATE') THEN
                    SELECT jsonb_object_agg(field.key, field.value)
                    INTO new_values
                    FROM jsonb_each(to_jsonb(NEW)) AS field
                    WHERE field.key = ANY (allowed_fields);
                END IF;

                IF TG_OP = 'UPDATE'
                   AND old_values IS NOT DISTINCT FROM new_values THEN
                    RETURN NULL;
                END IF;

                IF TG_OP = 'DELETE' THEN
                    key_values := to_jsonb(OLD);
                ELSE
                    key_values := to_jsonb(NEW);
                END IF;

                FOREACH key_column IN ARRAY TG_ARGV
                LOOP
                    row_key := row_key ||
                        jsonb_build_object(
                            key_column, key_values -> key_column
                        );
                END LOOP;

                INSERT INTO audit.row_changes (
                    schema_name, table_name, operation, row_pk,
                    actor_id, request_id, database_user, old_data, new_data
                )
                VALUES (
                    TG_TABLE_SCHEMA, TG_TABLE_NAME, TG_OP,
                    NULLIF(row_key, '{}'::jsonb),
                    NULLIF(current_setting('app.actor_id', true), ''),
                    NULLIF(current_setting('app.request_id', true), ''),
                    session_user, old_values, new_values
                );

                RETURN NULL;
            END;
            $$;

            SELECT audit.install_on_table('public."Assessment"'::regclass);
            SELECT audit.install_on_table('public."AssessmentRole"'::regclass);
            SELECT audit.install_on_table('public."AssessmentRegion"'::regclass);
            SELECT audit.install_on_table('public."AssessmentExperience"'::regclass);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "Use a reviewed forward migration to change the audit scope.");
    }
}