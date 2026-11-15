namespace ADAProjectVerticalSlice.Features.Assessments.GetAssessments
{
    public class GetAssessmentsResponse
    {
        public Guid AssessmentId { get; set; }
        public string AssessmentName { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
