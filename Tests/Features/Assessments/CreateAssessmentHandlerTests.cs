using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Features.Assessments.CreateAssessment;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Infrastructure.Messaging;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Runtime.ConstrainedExecution;

namespace ADAProjectAPIVerticalSlice.Tests.Features.Assessments
{
    [TestClass]
    public class CreateAssessmentHandlerTests
    {
        private ApplicationDbContext _dbContext = null!;
        private IValidator<CreateAssessment.Command> _validator = null!;
        private CreateAssessment.Handler _handler = null!;
        private Mock<IRabbitMqPublisher> _publisher = null!;

        private Role _existingRole = null!;
        private User _testUser = null!;
        private Company _testCompany = null!;
        private Survey _testSurvey = null!;

        // Fælles testdata og dependencies bliver oprettet før hver test.
        [TestInitialize]
        public async Task Setup()
        {
            // Opretter en ny InMemory database, så hver test er isoleret.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            // Opretter validatoren.
            _validator = new CreateAssessment.Validator();

            // Mock af RabbitMQ publisher.
            _publisher = new Mock<IRabbitMqPublisher>();

            // Opretter Handleren med alle dependencies.
            _handler = new CreateAssessment.Handler(
                _dbContext,
                _validator,
                _publisher.Object);

            // Opretter test Company.
            _testCompany = new Company
            {
                CompanyId = Guid.NewGuid(),
                CompanyName = "Test Company"
            };

            // Opretter test User.
            _testUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "test@test.dk",
                Email = "test@test.dk",
                FullName = "Test Bruger",
                CompanyId = _testCompany.CompanyId,
                Company = _testCompany
            };

            // Opretter test Survey.
            _testSurvey = new Survey
            {
                SurveyId = Guid.NewGuid(),
                Title = "ADA Survey",
                Description = "Test Survey"
            };

            // Opretter en eksisterende rolle.
            _existingRole = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = "Employee"
            };

            _dbContext.Company.Add(_testCompany);
            _dbContext.Users.Add(_testUser);
            _dbContext.Survey.Add(_testSurvey);
            _dbContext.Role.Add(_existingRole);

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
                SurveyId = _testSurvey.SurveyId,

                RoleNames = new List<string>
                {
                    "Employee"
                },

                Regions = new List<Region>
                {
                    Region.Americas
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
                .Include(a => a.Application)
                .Include(a => a.User)
                .Include(a => a.Survey)
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
                Region.Americas,
                assessment.Regions[0]);

            Assert.AreEqual(
                1,
                assessment.Experiences.Count);

            Assert.AreEqual(
                Experience.From1To2Years,
                assessment.Experiences[0]);

            Assert.AreEqual(
                _testUser.Id,
                assessment.UserId);

            Assert.AreEqual(
                _testSurvey.SurveyId,
                assessment.SurveyId);
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
                SurveyId = _testSurvey.SurveyId,
                RoleNames = new List<string>(),
                Regions = new List<Region>(),
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
                SurveyId = _testSurvey.SurveyId,

                RoleNames = new List<string>
                {
                    "Employee"
                },

                Regions = new List<Region>
                {
                    Region.Americas
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
            var application = await _dbContext.Application
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
                SurveyId = _testSurvey.SurveyId,

                RoleNames = new List<string>
                {
                    "New Role"
                },

                Regions = new List<Region>
                {
                    Region.Americas
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
            var role = await _dbContext.Role
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
                SurveyId = _testSurvey.SurveyId,

                RoleNames = new List<string>
                {
                    "Employee"
                },

                Regions = new List<Region>
                {
                    Region.Americas
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
            var roles = await _dbContext.Role
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
                SurveyId = _testSurvey.SurveyId,

                RoleNames = new List<string>
                {
                    "Employee"
                },

                Regions = new List<Region>
                {
                    Region.Americas
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
