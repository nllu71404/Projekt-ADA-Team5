namespace ADAProjectAPIVerticalSlice.Features.Assessment.CreateAssessment
{
    public class CreateAssessmentRequest
    {
        public string AssessmentName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
