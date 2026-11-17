namespace ADAProjectAPIVerticalSlice.Entities;

public sealed class PeopleContact
{
    public Guid PeopleContactId { get; set; }

    // Kontakten tilhører det firma, som den loggede administrator arbejder for.
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string Email { get; set; } = string.Empty;

    // Bruges til at finde dubletter uden forskel på store og små bogstaver.
    public string NormalizedEmail { get; set; } = string.Empty;

    public string? FullName { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Role { get; set; }
    public string? Region { get; set; }

    // Kontor og region gemmes hver for sig, så de kan bruges som separate filtre.
    public string? Office { get; set; }

    public decimal? YearsOfExperience { get; set; }
    public string? ExperienceLevel { get; set; }

    // Nye kontakter fra samme upload får det samme import-id.
    public Guid ImportBatchId { get; set; }
    public DateTime ImportedAtUtc { get; set; }
}