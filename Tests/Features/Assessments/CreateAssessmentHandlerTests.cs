using ADAProjectAPIVerticalSlice.Database;
using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Features.Assessments.CreateAssessment;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADAProjectAPIVerticalSlice.Tests.Features.Assessments
{
    [TestClass]
    public class CreateAssessmentHandlerTests
    {
        private ApplicationDbContext _dbContext = null!;
        private IValidator<CreateAssessment.Command> _validator = null!;
        private CreateAssessment.Handler _handler = null!;

        private Region _region = null!;
        private Role _existingRole = null!;

        // Fælles testdata og dependencies bliver oprettet før hver test.
        [TestInitialize]
        public async Task Setup()
        {
            // Opretter en ny InMemory database, så hver test er isoleret.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            // Opretter validatoren, som bruges af Handleren.
            _validator = new CreateAssessment.Validator();

            // Opretter Handleren med database og validator som dependencies.
            _handler = new CreateAssessment.Handler(
                _dbContext,
                _validator);

            // Opretter en region, som bruges af flere tests.
            _region = new Region
            {
                RegionId = Guid.NewGuid(),
                RegionName = "Americas"
            };

            // Opretter en eksisterende rolle, som bruges til at teste genbrug af roller.
            _existingRole = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = "Employee"
            };

            _dbContext.Regions.Add(_region);
            _dbContext.Roles.Add(_existingRole);

            await _dbContext.SaveChangesAsync();
        }

        [TestMethod]
        public async Task Handle_ValidCommand_CreatesAssessment()
        {
            // Arrange
            var command = new CreateAssessment.Command
            {
                AssessmentName = "ADA Measurement 2026",
                StartDate = new DateTime(2026, 9, 10),
                EndDate = new DateTime(2026, 10, 10),
                ApplicationName = "Microsoft Teams",

                RoleNames = new List<string>
                {
                    "Employee"
                },

                RegionIds = new List<Guid>
                {
                    _region.RegionId
                },

                Experiences = new List<Experience>
                {
                    Experience.From1To2Years
                }
            };

            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);

            var assessment = await _dbContext.Assessments
                .Include(a => a.Roles)
                .Include(a => a.Regions)
                .Include(a => a.Application)
                .FirstOrDefaultAsync();

            Assert.IsNotNull(assessment);

            Assert.AreEqual(
                "ADA Measurement 2026",
                assessment.AssessmentName);

            Assert.AreEqual(
                new DateTime(2026, 9, 10),
                assessment.StartDate);

            Assert.AreEqual(
                new DateTime(2026, 10, 10),
                assessment.EndDate);

            Assert.AreEqual(
                "Microsoft Teams",
                assessment.Application.ApplicationName);

            Assert.AreEqual(
                1,
                assessment.Roles.Count);

            Assert.AreEqual(
                "Employee",
                assessment.Roles[0].RoleName);

            Assert.AreEqual(
                1,
                assessment.Regions.Count);

            Assert.AreEqual(
                "Americas",
                assessment.Regions[0].RegionName);
        }

        [TestMethod]
        public async Task Handle_InvalidCommand_ReturnsValidationFailure()
        {
            // Arrange
            var command = new CreateAssessment.Command
            {
                AssessmentName = "",
                StartDate = new DateTime(2026, 10, 10),
                EndDate = new DateTime(2026, 9, 10),
                ApplicationName = "",
                RoleNames = new List<string>(),
                RegionIds = new List<Guid>(),
                Experiences = new List<Experience>()
            };

            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsFailure);

            Assert.AreEqual(
                "CreateAssessment.Validation",
                result.Error.Code);
        }

        [TestMethod]
        public async Task Handle_ApplicationDoesNotExist_CreatesApplication()
        {
            // Arrange
            var command = new CreateAssessment.Command
            {
                AssessmentName = "Test Assessment",
                StartDate = new DateTime(2026, 9, 10),
                EndDate = new DateTime(2026, 10, 10),
                ApplicationName = "New Application",

                RoleNames = new List<string>
                {
                    "Employee"
                },

                RegionIds = new List<Guid>
                {
                    _region.RegionId
                },

                Experiences = new List<Experience>
                {
                    Experience.From1To2Years
                }
            };

            // Act
            await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            var application = await _dbContext.Applications
                .FirstOrDefaultAsync(
                    a => a.ApplicationName == "New Application");

            Assert.IsNotNull(application);

            Assert.AreEqual(
                "New Application",
                application.ApplicationName);
        }

        [TestMethod]
        public async Task Handle_RoleDoesNotExist_CreatesRole()
        {
            // Arrange
            var command = new CreateAssessment.Command
            {
                AssessmentName = "Test Assessment",
                StartDate = new DateTime(2026, 9, 10),
                EndDate = new DateTime(2026, 10, 10),
                ApplicationName = "Microsoft Teams",

                RoleNames = new List<string>
                {
                    "New Role"
                },

                RegionIds = new List<Guid>
                {
                    _region.RegionId
                },

                Experiences = new List<Experience>
                {
                    Experience.From1To2Years
                }
            };

            // Act
            await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(
                    r => r.RoleName == "New Role");

            Assert.IsNotNull(role);

            Assert.AreEqual(
                "New Role",
                role.RoleName);
        }

        [TestMethod]
        public async Task Handle_ExistingRole_ReusesExistingRole()
        {
            // Arrange
            var command = new CreateAssessment.Command
            {
                AssessmentName = "Test Assessment",
                StartDate = new DateTime(2026, 9, 10),
                EndDate = new DateTime(2026, 10, 10),
                ApplicationName = "Microsoft Teams",

                RoleNames = new List<string>
                {
                    "Employee"
                },

                RegionIds = new List<Guid>
                {
                    _region.RegionId
                },

                Experiences = new List<Experience>
                {
                    Experience.From1To2Years
                }
            };

            // Act
            await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            var roles = await _dbContext.Roles
                .ToListAsync();

            Assert.AreEqual(
                1,
                roles.Count);

            Assert.AreEqual(
                _existingRole.RoleId,
                roles[0].RoleId);

            Assert.AreEqual(
                "Employee",
                roles[0].RoleName);
        }

        [TestMethod]
        public async Task Handle_MultipleExperiencesSelected_AddsAllExperiencesToAssessment()
        {
            // Arrange
            var command = new CreateAssessment.Command
            {
                AssessmentName = "ADA Measurement 2026",
                StartDate = new DateTime(2026, 9, 10),
                EndDate = new DateTime(2026, 10, 10),
                ApplicationName = "Microsoft Teams",

                RoleNames = new List<string>
        {
            "Employee"
        },

                RegionIds = new List<Guid>
        {
            _region.RegionId
        },

                Experiences = new List<Experience>
        {
            Experience.LessThan1Year,
            Experience.From1To2Years,
            Experience.From3To5Years
        }
            };

            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);

            var assessment = await _dbContext.Assessments
                .FirstOrDefaultAsync();

            Assert.IsNotNull(assessment);

            Assert.AreEqual(
                3,
                assessment.Experiences.Count);

            CollectionAssert.AreEquivalent(
                new[]
                {
            Experience.LessThan1Year,
            Experience.From1To2Years,
            Experience.From3To5Years
                },
                assessment.Experiences);
        }
    }
}
