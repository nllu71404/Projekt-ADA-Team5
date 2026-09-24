namespace ADAProjectAPIVerticalSlice.Entities
{
    public class Question
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionPolarity { get; set; } = string.Empty;

        // Foreign keys
        public Guid SurveyId { get; set; }
        public Guid ThemeId { get; set; }

        // Navigation properties
        public Survey Survey { get; set; } = null!;
        public Theme Theme { get; set; } = null!;
    }
}
