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

        // Matcher handlerens nuværende hardcodede SurveyId.
        private static readonly Guid TestSurveyId =
            Guid.Parse("22222222-2222-2222-2222-222222222222");

        [TestInitialize]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _validator = new CreateAssessment.Validator();
            _publisher = new Mock<IRabbitMqPublisher>();

            _emailTimingCalculator = new EmailTimingCalculator();

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

            _testCompany = new Company
            {
                CompanyId = Guid.NewGuid(),
                CompanyName = "Test Company"
            };

            _testUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "test@test.dk",
                Email = "test@test.dk",
                FullName = "Test Bruger",
                CompanyId = _testCompany.CompanyId,
                Company = _testCompany
            };

            _testSurvey = new Survey
            {
                SurveyId = TestSurveyId,
                Title = "ADA Survey",
                Description = "Test Survey"
            };

            _existingRole = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = "Employee"
            };

            _existingRegion = new Region
            {
                RegionId = Guid.NewGuid(),
                RegionName = "Americas"
            };

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

            _dbContext.Companies.Add(_testCompany);
            _dbContext.Users.Add(_testUser);
            _dbContext.Surveys.Add(_testSurvey);
            _dbContext.Roles.Add(_existingRole);
            _dbContext.Regions.Add(_existingRegion);

            _dbContext.Experiences.AddRange(
                _experienceLessThan1Year,
                _experienceFrom1To2Years,
                _experienceFrom3To5Years);

            await _dbContext.SaveChangesAsync();
        }

        private CreateAssessment.Command CreateValidCommand()
        {
            return new CreateAssessment.Command
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
        }

        [TestMethod]
        public async Task Handle_ValidCommand_CreatesAssessment()
        {
            // Arrange
            var command = CreateValidCommand();

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
            Assert.IsNotNull(assessment.Application);
            Assert.AreEqual(
                "Microsoft Teams",
                assessment.Application.ApplicationName);

            // Role
            Assert.AreEqual(1, assessment.Roles.Count);
            Assert.AreEqual(
                "Employee",
                assessment.Roles[0].RoleName);

            // Region
            Assert.AreEqual(1, assessment.Regions.Count);
            Assert.AreEqual(
                _existingRegion.RegionId,
                assessment.Regions[0].RegionId);

            Assert.AreEqual(
                "Americas",
                assessment.Regions[0].RegionName);

            // Experience
            Assert.AreEqual(1, assessment.Experiences.Count);
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
            Assert.IsNotNull(assessment.Survey);
            Assert.AreEqual(
                TestSurveyId,
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
                string.IsNullOrWhiteSpace(respondent.AccessToken));

            // Event
            _publisher.Verify(
                p => p.PublishAsync(
                    It.Is<AssessmentCreated>(e =>
                        e.AssessmentId == assessment.AssessmentId),
                    "assessment-created",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Handle_InvalidCommand_ReturnsValidationError()
        {
            // Arrange
            var command = CreateValidCommand();

            command.AssessmentName = "";
            command.ApplicationName = "";
            command.StartDate = new DateTime(2026, 10, 10);
            command.EndDate = new DateTime(2026, 9, 10);
            command.RoleNames.Clear();
            command.RegionIds.Clear();
            command.ExperienceIds.Clear();

            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            Assert.IsFalse(result.IsSuccess);

            Assert.AreEqual(
                0,
                await _dbContext.Assessments.CountAsync());

            _publisher.Verify(
                p => p.PublishAsync(
                    It.IsAny<AssessmentCreated>(),
                    "assessment-created",
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Handle_ApplicationDoesNotExist_CreatesApplication()
        {
            // Arrange
            var command = CreateValidCommand();
            command.ApplicationName = "New Test Application";

            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);

            var application = await _dbContext.Applications
                .FirstOrDefaultAsync(
                    a => a.ApplicationName == "New Test Application");

            Assert.IsNotNull(application);

            var assessment = await _dbContext.Assessments
                .FirstOrDefaultAsync();

            Assert.IsNotNull(assessment);
            Assert.AreEqual(
                application.ApplicationId,
                assessment.ApplicationId);
        }

        [TestMethod]
        public async Task Handle_RoleDoesNotExist_CreatesRole()
        {
            // Arrange
            var command = CreateValidCommand();
            command.RoleNames = new List<string>
            {
                "New Test Role"
            };

            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);

            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(
                    r => r.RoleName == "New Test Role");

            Assert.IsNotNull(role);

            var assessment = await _dbContext.Assessments
                .Include(a => a.Roles)
                .FirstOrDefaultAsync();

            Assert.IsNotNull(assessment);
            Assert.AreEqual(1, assessment.Roles.Count);

            Assert.AreEqual(
                role.RoleId,
                assessment.Roles[0].RoleId);
        }

        [TestMethod]
        public async Task Handle_ExistingRole_ReusesExistingRole()
        {
            // Arrange
            var command = CreateValidCommand();

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
            Assert.AreEqual(1, assessment.Roles.Count);

            Assert.AreEqual(
                _existingRole.RoleId,
                assessment.Roles[0].RoleId);

            Assert.AreEqual(
                1,
                await _dbContext.Roles.CountAsync());
        }

        [TestMethod]
        public async Task Handle_MultipleExperiencesSelected_AddsAllExperiencesToAssessment()
        {
            // Arrange
            var command = CreateValidCommand();

            command.ExperienceIds = new List<Guid>
            {
                _experienceLessThan1Year.ExperienceId,
                _experienceFrom1To2Years.ExperienceId,
                _experienceFrom3To5Years.ExperienceId
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
            Assert.AreEqual(3, assessment.Experiences.Count);

            var experienceIds = assessment.Experiences
                .Select(e => e.ExperienceId)
                .ToList();

            CollectionAssert.AreEquivalent(
                command.ExperienceIds,
                experienceIds);
        }

        [TestMethod]
        public async Task Handle_RegionDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();

            command.RegionIds = new List<Guid>
            {
                Guid.NewGuid()
            };

            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            Assert.IsFalse(result.IsSuccess);

            Assert.AreEqual(
                0,
                await _dbContext.Assessments.CountAsync());
        }

        [TestMethod]
        public async Task Handle_ExperienceDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();

            command.ExperienceIds = new List<Guid>
            {
                Guid.NewGuid()
            };

            // Act
            var result = await _handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            Assert.IsFalse(result.IsSuccess);

            Assert.AreEqual(
                0,
                await _dbContext.Assessments.CountAsync());
        }
    }
}