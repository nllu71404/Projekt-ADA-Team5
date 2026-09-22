namespace ADAProjectAPIVerticalSlice.Entities
{
    public class Assessment
    {
        public Guid AssessmentId { get; set; }

        public string AssessmentName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<Role> Roles { get; set; } = new List<Role>();

        public List<Region> Regions { get; set; } = new List<Region>();

        public List<Experience> Experiences { get; set; } = new List<Experience>();


        //Foreign keys
        public string UserId { get; set; } //IdentityUser bruger string til UserId
        
        public Guid ApplicationId { get; set; }

        //Navigation properties
        public User User { get; set; } = null!;
        public Application Application { get; set; } = null!;
    }
}
