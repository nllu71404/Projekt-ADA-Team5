namespace ADAProjectAPIVerticalSlice.Entities
{
    public class UserSession
    {
        public Guid Id { get; set; }

        // den beskyttede authentication-ticket, der bruges til at validere sessionen
        public byte[] Ticket { get; set; } = [];

        public DateTime Created { get; set; } 

        public DateTime LastActivity { get; set; }

        public DateTime Expires { get; set; }

        public DateTime? Revoked { get; set; }

        // Foreign key
        public string UserId { get; set; } = string.Empty;

        // Navigation property
        public User User { get; set; } = null!;
    }
}
