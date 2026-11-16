namespace ADAProjectAPIVerticalSlice.Features.Authentication.GetCurrentUser
{
    public class GetCurrentUserResponse
    {
        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();

       
    }
}
