using Microsoft.AspNetCore.Identity;

namespace ADAProjectAPIVerticalSlice.Entities
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;

        //Foreign key
        public Guid CompanyId { get; set; }

        //Navigation property
        public Company Company { get; set; } = null!;
    }
}
