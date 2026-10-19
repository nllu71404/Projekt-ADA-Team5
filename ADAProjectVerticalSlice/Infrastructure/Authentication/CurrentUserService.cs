using System.Security.Claims;

namespace ADAProjectAPIVerticalSlice.Infrastructure.Authentication
{
    public class CurrentUserService : ICurrentUserService
    {
        // HttpContextAccessor bruges til at få adgang til den aktuelle HTTP-kontekst, som indeholder oplysninger om den aktuelle bruger.
        private readonly IHttpContextAccessor _httpContextAccessor;

        
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Læser brugerens Id fra JWT-tokenet, som er gemt i HTTP-konteksten. Hvis brugeren ikke er logget ind, returneres null.
        public string? UserId =>
            _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
