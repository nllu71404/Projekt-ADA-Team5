using ADA_Contracts.Events;
using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Features.Assessments.CreateAssessment;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Infrastructure.Messaging;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Tests.Features.Assessments.CreateAssessmentTests
{
    [TestClass]
    public class CreateAssessmentHandlerTests
    {
        private ApplicationDbContext _dbContext = null!;
        private IValidator<CreateAssessment.Command> _validator = null!;
        private CreateAssessment.Handler _handler = null!;
        private Mock<IRabbitMqPublisher> _publisher = null!;
        private EmailTimingCalculator _emailTimingCalculator = null!;

        private Role _existingRole = null!;
        private Region _existingRegion = null!;
        private Experience _experienceLessThan1Year = null!;
        private Experience _experienceFrom1To2Years = null!;
        private Experience _experienceFrom3To5Years = null!;

        private User _testUser = null!;
        private Company _testCompany = null!;
        private Survey _testSurvey = null!;


        [TestInitialize]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _validator = new CreateAssessment.Validator();

            _publisher = new Mock<IRabbitMqPublisher>();

            _publisher
                .Setup(p => p.PublishAsync(
                    It.IsAny<AssessmentCreated>(),
                    "assessment-created",
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _handler = new CreateAssessment.Handler(
                _dbContext,
                _validator,
                _publisher.Object,
                _emailTimingCalculator);

            // -------------------------
            // Company
            // -------------------------

            _testCompany = new Company
            {
                CompanyId = Guid.NewGuid(),
                CompanyName = "Test Company"
            };


            // -------------------------
            // User
            // -------------------------

            _testUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "test@test.dk",
                Email = "test@test.dk",
                FullName = "Test Bruger",
                CompanyId = _testCompany.CompanyId,
                Company = _testCompany
            };


            // -------------------------
            // Survey
            // -------------------------

            _testSurvey = new Survey
            {
                SurveyId = Guid.NewGuid(),
                Title = "ADA Survey",
                Description = "Test Survey"
            };


            // -------------------------
            // Role
            // -------------------------

            _existingRole = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = "Employee"
            };


            // -------------------------
            // Region
            // -------------------------

            _existingRegion = new Region
            {
                RegionId = Guid.NewGuid(),
                RegionName = "Americas"
            };


            // -------------------------
            // Experiences
            // -------------------------

            _experienceLessThan1Year = new Experience
            {
                ExperienceId = Guid.NewGuid(),
                ExperienceValue = "<1 år"
            };

            _experienceFrom1To2Years = new Experience
            {
                ExperienceId = Guid.NewGuid(),
                ExperienceValue = "1-2 år"
            };

            _experienceFrom3To5Years = new Experience
            {
                ExperienceId = Guid.NewGuid(),
                ExperienceValue = "3-5 år"
            };


            // -------------------------
            // Add test data
            // -------------------------

            _dbContext.Companies.Add(_testCompany);
            _dbContext.Users.Add(_testUser);
            _dbContext.Surveys.Add(_testSurvey);
            _dbContext.Roles.Add(_existingRole);

            _dbContext.Regions.Add(_existingRegion);

            _dbContext.Experiences.Add(_experienceLessThan1Year);
            _dbContext.Experiences.Add(_experienceFrom1To2Years);
            _dbContext.Experiences.Add(_experienceFrom3To5Years);

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

                RegionIds = new List<Guid>
                {
                    _existingRegion.RegionId
                },

                ExperienceIds = new List<Guid>
                {
                    _experienceFrom1To2Years.ExperienceId
                },

                RespondentEmails = new List<string>
                {
                    "respondent@test.dk"
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
                .Include(a => a.Experiences)
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


            // Application
            Assert.AreEqual(
                "Microsoft Teams",
                assessment.Application.ApplicationName);


            // Role
            Assert.AreEqual(
                1,
                assessment.Roles.Count);

            Assert.AreEqual(
                "Employee",
                assessment.Roles[0].RoleName);


            // Region
            Assert.AreEqual(
                1,
                assessment.Regions.Count);

            Assert.AreEqual(
                _existingRegion.RegionId,
                assessment.Regions[0].RegionId);

            Assert.AreEqual(
                "Americas",
                assessment.Regions[0].RegionName);


            // Experience
            Assert.AreEqual(
                1,
                assessment.Experiences.Count);

            Assert.AreEqual(
                _experienceFrom1To2Years.ExperienceId,
                assessment.Experiences[0].ExperienceId);

            Assert.AreEqual(
                "1-2 år",
                assessment.Experiences[0].ExperienceValue);


            // User
            Assert.AreEqual(
                _testUser.Id,
                assessment.UserId);


            // Survey
            Assert.AreEqual(
                _testSurvey.SurveyId,
                assessment.SurveyId);


            // Respondent
            var respondent = await _dbContext.Respondents
                .FirstOrDefaultAsync();

            Assert.IsNotNull(respondent);

            Assert.AreEqual(
                "respondent@test.dk",
                respondent.EmailAddress);

            Assert.AreEqual(
                assessment.AssessmentId,
                respondent.AssessmentId);

            Assert.IsFalse(
                string.IsNullOrEmpty(respondent.AccessToken));
        }


        [TestMethod]
        public async Task Handle_InvalidCommand_ReturnsValidationError()
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

                ExperienceIds = new List<Guid>(),

                RespondentEmails = new List<string>()
            };


            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);


            // Assert
            Assert.IsFalse(result.IsSuccess);
        }


        [TestMethod]
        public async Task Handle_ApplicationDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var command = new CreateAssessment.Command
            {
                AssessmentName = "ADA Measurement 2026",
                StartDate = new DateTime(2026, 9, 10),
                EndDate = new DateTime(2026, 10, 10),

                ApplicationName = "NonExisting Application",

                SurveyId = _testSurvey.SurveyId,

                RoleNames = new List<string>
                {
                    "Employee"
                },

                RegionIds = new List<Guid>
                {
                    _existingRegion.RegionId
                },

                ExperienceIds = new List<Guid>
                {
                    _experienceFrom1To2Years.ExperienceId
                }
            };


            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);


            // Assert
            Assert.IsFalse(result.IsSuccess);
        }


        [TestMethod]
        public async Task Handle_RoleDoesNotExist_ReturnsFailure()
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
                    "NonExistingRole"
                },

                RegionIds = new List<Guid>
                {
                    _existingRegion.RegionId
                },

                ExperienceIds = new List<Guid>
                {
                    _experienceFrom1To2Years.ExperienceId
                }
            };


            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);


            // Assert
            Assert.IsFalse(result.IsSuccess);
        }


        [TestMethod]
        public async Task Handle_ExistingRole_ReusesExistingRole()
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

                RegionIds = new List<Guid>
                {
                    _existingRegion.RegionId
                },

                ExperienceIds = new List<Guid>
                {
                    _experienceFrom1To2Years.ExperienceId
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
                .FirstOrDefaultAsync();

            Assert.IsNotNull(assessment);

            Assert.AreEqual(
                1,
                assessment.Roles.Count);

            Assert.AreEqual(
                _existingRole.RoleId,
                assessment.Roles[0].RoleId);
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

                RegionIds = new List<Guid>
                {
                    _existingRegion.RegionId
                },

                ExperienceIds = new List<Guid>
                {
                    _experienceLessThan1Year.ExperienceId,
                    _experienceFrom1To2Years.ExperienceId,
                    _experienceFrom3To5Years.ExperienceId
                }
            };


            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);


            // Assert
            Assert.IsTrue(result.IsSuccess);

            var assessment = await _dbContext.Assessments
                .Include(a => a.Experiences)
                .FirstOrDefaultAsync();

            Assert.IsNotNull(assessment);

            Assert.AreEqual(
                3,
                assessment.Experiences.Count);


            var experienceIds = assessment.Experiences
                .Select(e => e.ExperienceId)
                .ToList();


            CollectionAssert.AreEquivalent(
                new List<Guid>
                {
                    _experienceLessThan1Year.ExperienceId,
                    _experienceFrom1To2Years.ExperienceId,
                    _experienceFrom3To5Years.ExperienceId
                },
                experienceIds);
        }
    }
}
