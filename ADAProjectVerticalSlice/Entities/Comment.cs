namespace ADAProjectAPIVerticalSlice.Entities
{
    public class Comment
    {
        public Guid CommentId { get; set; }
        public string FreeForm { get; set; } = string.Empty;

        // Foreign keys
        public Guid ThemeId { get; set; }
        public Guid RespondentId { get; set; }

        // Navigation property
        public Theme Theme { get; set; } = null!;
        public Respondent Respondent { get; set; } = null!;
    }
}
