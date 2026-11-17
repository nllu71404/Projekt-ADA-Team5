namespace ADAProjectAPIVerticalSlice.Infrastructure.Database.Auditing;

public sealed class AssessmentAuditChange
{
    public long AuditId { get; set; }
    public DateTime ChangedAt { get; set; }
    public Guid CompanyId { get; set; }
    public string AssessmentId { get; set; } = "";
    public string Operation { get; set; } = "";
    public string FieldName { get; set; } = "";
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ActorId { get; set; }
    public string? RequestId { get; set; }
}