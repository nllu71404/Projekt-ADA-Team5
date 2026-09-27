namespace ADAProjectAPIVerticalSlice.Entities
{
    public class Theme
    {
        public Guid ThemeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Comment> Comments { get; set; } = new List<Comment>();

        // Foreign keys
        public Guid SurveyId { get; set; }

        // Navigation property
        public Survey Survey { get; set; } = null!;


    }
}
