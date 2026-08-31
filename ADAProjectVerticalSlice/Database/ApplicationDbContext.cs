using Microsoft.EntityFrameworkCore;
using ADAProjectAPIVerticalSlice.Features.Assessment;

namespace ADAProjectAPIVerticalSlice.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Assessment> Assessments { get; set; }
    }
}
