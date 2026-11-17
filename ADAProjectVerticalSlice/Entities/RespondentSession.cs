using ADAProjectAPIVerticalSlice.Entities;

namespace ADAProjectAPIVerticalSlice.Entities
{
    public class RespondentSession
    {
        public Guid Id { get; set; }

        // Beskyttet authentication-ticket
        public byte[] Ticket { get; set; } = [];

        public DateTime Created { get; set; }

        public DateTime LastActivity { get; set; }

        public DateTime Expires { get; set; }

        public DateTime? Revoked { get; set; }

        // Foreign key til respondenten
        public Guid RespondentId { get; set; }

        // Navigation property
        public Respondent Respondent { get; set; } = null!;
    }
}
