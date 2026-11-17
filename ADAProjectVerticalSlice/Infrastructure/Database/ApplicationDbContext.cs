using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Database.Auditing;


namespace ADAProjectAPIVerticalSlice.Infrastructure.Database
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Assessment> Assessments { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Region> Regions { get; set; }

        public DbSet<Experience> Experiences { get; set; }

        public DbSet<Survey> Surveys { get; set; }

        public DbSet<Theme> Themes { get; set; }

        public DbSet<Question> Questions { get; set; }

        public DbSet<Respondent> Respondents { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<AssessmentAuditChange> AssessmentAuditChanges => Set<AssessmentAuditChange>();

        public DbSet<PeopleContact> PeopleContacts => Set<PeopleContact>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Company>().ToTable("Company");
            modelBuilder.Entity<Application>().ToTable("Application");
            modelBuilder.Entity<Assessment>().ToTable("Assessment");
            modelBuilder.Entity<Region>().ToTable("Regions");
            modelBuilder.Entity<Experience>().ToTable("Experiences");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Question>().ToTable("Question");
            modelBuilder.Entity<Theme>().ToTable("Theme");
            modelBuilder.Entity<Survey>().ToTable("Survey");
            modelBuilder.Entity<Respondent>().ToTable("Respondent");
            modelBuilder.Entity<Answer>().ToTable("Answer");
            modelBuilder.Entity<Comment>().ToTable("Comment");
            modelBuilder.Entity<UserSession>().ToTable("UserSessions");

            // Survey
            modelBuilder.Entity<Survey>()
                .ToTable("Survey");

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
            modelBuilder.Entity<Role>().ToTable("Roles");

            modelBuilder.Entity<Assessment>()
                .HasMany(a => a.Roles)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "AssessmentRole",
                    j => j
                        .HasOne<Role>()
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK_AssessmentRole_Roles_RoleId"),
                    j => j
                        .HasOne<Assessment>()
                        .WithMany()
                        .HasForeignKey("AssessmentId")
                        .HasConstraintName("FK_AssessmentRole_Assessment_AssessmentId"),
                    j =>
                    {
                        j.HasKey("AssessmentId", "RoleId");
                        j.ToTable("AssessmentRole");
                    });

            //Assessment <-> Region
            modelBuilder.Entity<Assessment>()
                .HasMany(a => a.Regions)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "AssessmentRegion",
                    j => j
                        .HasOne<Region>()
                        .WithMany()
                        .HasForeignKey("RegionId")
                        .HasConstraintName("FK_AssessmentRegion_Regions_RegionId"),
                    j => j
                        .HasOne<Assessment>()
                        .WithMany()
                        .HasForeignKey("AssessmentId")
                        .HasConstraintName("FK_AssessmentRegion_Assessment_AssessmentId"),
                    j =>
                    {
                        j.HasKey("AssessmentId", "RegionId");
                        j.ToTable("AssessmentRegion");
                    });

            // Assessment -> Respondent
            modelBuilder.Entity<Respondent>()
                .HasOne(r => r.Assessment)
                .WithMany(a => a.Respondents)
                .HasForeignKey(r => r.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);


            //Region
            modelBuilder.Entity<Region>()
                .HasIndex(r => r.RegionName)
                .IsUnique();

            //Experience
            modelBuilder.Entity<Assessment>()
                .HasMany(a => a.Experiences)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "AssessmentExperience",
                    j => j
                        .HasOne<Experience>()
                        .WithMany()
                        .HasForeignKey("ExperienceId")
                        .HasConstraintName("FK_AssessmentExperience_Experiences_ExperienceId"),
                    j => j
                        .HasOne<Assessment>()
                        .WithMany()
                        .HasForeignKey("AssessmentId")
                        .HasConstraintName("FK_AssessmentExperience_Assessment_AssessmentId"),
                    j =>
                    {
                        j.HasKey("AssessmentId", "ExperienceId");
                        j.ToTable("AssessmentExperience");
                    });

            modelBuilder.Entity<Experience>()
                .HasIndex(e => e.ExperienceValue)
                .IsUnique();


            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.ToTable("OutboxMessages");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Type)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.RoutingKey)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Payload)
                    .IsRequired();

                entity.HasIndex(x => new
                {
                    x.ProcessedOnUtc,
                    x.CreatedOnUtc
                });
            });


            // User -> UserSession
            modelBuilder.Entity<UserSession>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Én auditpost kan give flere rækker i visningen, én pr. ændret felt.
            modelBuilder.Entity<AssessmentAuditChange>()
                .HasNoKey()
                .ToView("assessment_field_changes", "audit");

            modelBuilder.Entity<PeopleContact>(contact =>
            {
                contact.ToTable("PeopleContacts");
                contact.HasKey(person => person.PeopleContactId);

                contact.Property(person => person.Email).HasMaxLength(254);
                contact.Property(person => person.NormalizedEmail).HasMaxLength(254);
                contact.Property(person => person.FullName).HasMaxLength(120);
                contact.Property(person => person.JobTitle).HasMaxLength(120);
                contact.Property(person => person.Department).HasMaxLength(120);
                contact.Property(person => person.Role).HasMaxLength(120);
                contact.Property(person => person.Region).HasMaxLength(120);
                contact.Property(person => person.Office).HasMaxLength(120);
                contact.Property(person => person.ExperienceLevel).HasMaxLength(120);

                // En adresse må kun findes én gang inden for det samme firma.
                contact.HasIndex(person => new { person.CompanyId, person.NormalizedEmail })
                    .IsUnique()
                    .HasDatabaseName("UX_PeopleContacts_CompanyId_NormalizedEmail");

                // Firmaet skal have sine kontakter fjernet, før det kan slettes.
                contact.HasOne(person => person.Company)
                    .WithMany()
                    .HasForeignKey(person => person.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}

