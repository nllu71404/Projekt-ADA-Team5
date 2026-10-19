using ADAProjectAPIVerticalSlice.Entities;

namespace ADAProjectAPIVerticalSlice.Infrastructure.Authentication;

public interface IJwtTokenService
{
    Task<string> CreateTokenAsync(User user);
}
