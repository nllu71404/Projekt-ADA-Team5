namespace ADAProjectAPIVerticalSlice.Features.Assessment
{
    public class Assessment
    {
        public Guid AssessmentId { get; set; }

        public string AssessmentName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
