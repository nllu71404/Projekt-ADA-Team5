namespace ADA.Frontend.Services;

public class AuthSession
{
    public event Action? OnChange;

    public string? CookieHeader { get; set; }

    public string? CsrfToken { get; set; }

    //Almindelige brugere
    public bool IsAuthenticated =>
        !string.IsNullOrWhiteSpace(CookieHeader) &&
        CookieHeader.Contains("ADA.Auth=");

    //Respondenter
    public bool IsRespondentAuthenticated =>
    !string.IsNullOrWhiteSpace(CookieHeader) &&
    CookieHeader.Contains("ADA.Respondent=");

    public void NotifyChanged()
    {
        OnChange?.Invoke();
    }

    public void Clear()
    {
        CookieHeader = null;
        CsrfToken = null;

        NotifyChanged();
    }
}