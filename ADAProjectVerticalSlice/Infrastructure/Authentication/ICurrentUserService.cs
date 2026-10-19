namespace ADAProjectAPIVerticalSlice.Infrastructure.Authentication
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
    }
}
