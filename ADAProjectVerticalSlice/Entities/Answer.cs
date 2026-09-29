namespace ADAProjectAPIVerticalSlice.Entities
{
    public class Answer
    {
        public Guid AnswerId { get; set; }
        public int Points { get; set; }

        // Foreign keys
        public Guid QuestionId { get; set; }
        public Guid RespondentId { get; set; }

        // Navigation properties
        public Question Question { get; set; } = null!;
        public Respondent Respondent { get; set; } = null!;
    }
}
