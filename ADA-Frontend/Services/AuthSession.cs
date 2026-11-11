namespace ADA.Frontend.Services;

public class AuthSession
{
    // Indeholder cookies fra backend, som skal sendes med API-requests.
    public string? CookieHeader { get; set; }

    // CSRF request-token fra backend.
    public string? CsrfToken { get; set; }

    // Returnerer true, hvis brugeren er logget ind. Gør det muligt for frontend at vise/hide elementer baseret på login-status.
    public bool IsAuthenticated =>
        !string.IsNullOrWhiteSpace(CookieHeader) &&
        CookieHeader.Contains("ADA.Auth=");
}
