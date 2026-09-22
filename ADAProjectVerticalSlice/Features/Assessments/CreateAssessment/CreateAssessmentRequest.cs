using ADAProjectAPIVerticalSlice.Entities;


namespace ADAProjectAPIVerticalSlice.Features.Assessments.CreateAssessment

{
    public class CreateAssessmentRequest
    {
        public string AssessmentName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string ApplicationName { get; set; } = string.Empty;

        public List<string> RoleNames { get; set; } = new();

        public List<Guid> RegionIds { get; set; } = new();

        public List<Experience> Experiences { get; set; } = new();

    }
}
