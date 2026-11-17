using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAProjectAPIVerticalSlice.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentAuditFieldChangesView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
    ALTER TABLE audit.row_changes
    ADD COLUMN IF NOT EXISTS company_id uuid;
    CREATE VIEW audit.assessment_field_changes AS
    SELECT
        a.id AS "AuditId",
        a.changed_at AS "ChangedAt",
        a.company_id AS "CompanyId",
        a.row_pk ->> 'AssessmentId' AS "AssessmentId",
        a.operation AS "Operation",
        field.label AS "FieldName",
        a.old_data ->> field.json_key AS "OldValue",
        a.new_data ->> field.json_key AS "NewValue",
        a.actor_id AS "ActorId",
        a.request_id AS "RequestId"
    FROM audit.row_changes AS a
    JOIN (
        VALUES
            ('Assessment', 'AssessmentName', 'Survey name'),
            ('Assessment', 'StartDate', 'Start date'),
            ('Assessment', 'EndDate', 'End date'),
            ('AssessmentRole', 'RoleId', 'Role'),
            ('AssessmentRegion', 'RegionId', 'Region'),
            ('AssessmentExperience', 'ExperienceId', 'Experience level')
    ) AS field(table_name, json_key, label)
        ON field.table_name = a.table_name
    WHERE a.schema_name = 'public'
      -- Kundevisningen kræver et firma-id, der blev gemt med auditposten.
      AND a.company_id IS NOT NULL
      AND (a.row_pk ->> 'AssessmentId') IS NOT NULL
      AND (a.old_data -> field.json_key)
          IS DISTINCT FROM (a.new_data -> field.json_key);
    """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW audit.assessment_field_changes;");
        }
    }
}
