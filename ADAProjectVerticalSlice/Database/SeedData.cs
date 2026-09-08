using ADAProjectAPIVerticalSlice.Entities;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Database
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
            IServiceProvider services)
        {
            var dbContext =
                services.GetRequiredService<ApplicationDbContext>();

            // ==========================================
            // 1. Opret test Company
            // ==========================================

            var company = await dbContext.Companies
                .FirstOrDefaultAsync(c =>
                    c.CompanyName == "Test Company");

            if (company == null)
            {
                company = new Company
                {
                    CompanyId = Guid.NewGuid(),
                    CompanyName = "Test Company"
                };

                dbContext.Companies.Add(company);

                await dbContext.SaveChangesAsync();
            }


            // ==========================================
            // 2. Opret test User
            // ==========================================

            var user = await dbContext.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == "test@test.dk");

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "test@test.dk",
                    Email = "test@test.dk",
                    FullName = "Test Bruger",

                    // Knyt brugeren til Test Company
                    CompanyId = company.CompanyId
                };

                dbContext.Users.Add(user);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}