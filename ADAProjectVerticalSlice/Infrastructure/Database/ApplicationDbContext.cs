using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ADAProjectAPIVerticalSlice.Entities;


namespace ADAProjectAPIVerticalSlice.Infrastructure.Database
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Assessment> Assessments { get; set; }

        public DbSet<Application> Application { get; set; }

        public DbSet<Company> Company { get; set; }

        public DbSet<Role> Role { get; set; }

        public DbSet<Region> Region { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User -> Company
            modelBuilder.Entity<User>()
                .HasOne(u => u.Company)
                .WithMany()
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);


            // Assessment -> User
            modelBuilder.Entity<Assessment>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // Assessment -> Application
            modelBuilder.Entity<Assessment>()
                .HasOne(a => a.Application)
                .WithMany()
                .HasForeignKey(a => a.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);


            //Assessment <-> Role
            modelBuilder.Entity<Assessment>()
                .HasMany(a => a.Roles)
                .WithMany();

            //Assessment <-> Region
            modelBuilder.Entity<Assessment>()
                .HasMany(a => a.Regions)
                .WithMany();


            //Region
            modelBuilder.Entity<Region>()
                 .HasIndex(r => r.RegionName)
                 .IsUnique();


        }
    }
}

