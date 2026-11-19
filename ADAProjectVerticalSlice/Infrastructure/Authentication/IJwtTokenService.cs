using ADAProjectAPIVerticalSlice.Entities;

namespace ADAProjectAPIVerticalSlice.Infrastructure.Authentication;

public interface IJwtTokenService
{
    // Til oprettelse af JWT-token til normal brugere (f.eks. administratorer)
    Task<string> CreateTokenAsync(User user);

    // Til oprettelse af en token til en respondent
    Task<string> CreateRespondentTokenAsync(
    string respondentId,
    string email);
}
