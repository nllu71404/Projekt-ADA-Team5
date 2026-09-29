namespace ADAProjectAPIVerticalSlice.Entities
{
    public class Respondent
    {
        public Guid RespondentId { get; set; }
        public bool HasAnswered { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;

        // Foreign key
        public Guid AssessmentId { get; set; }

        // Navigation property
        public Assessment Assessment { get; set; } = null!;
    }
}
