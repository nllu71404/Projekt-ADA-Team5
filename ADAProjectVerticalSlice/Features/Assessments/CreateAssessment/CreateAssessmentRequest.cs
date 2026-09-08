namespace ADAProjectAPIVerticalSlice.Features.Assessments.CreateAssessment
{
    public class CreateAssessmentRequest
    {
        public string AssessmentName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string ApplicationName { get; set; } = string.Empty;

    }
}
