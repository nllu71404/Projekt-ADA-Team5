using ADA_Contracts.Events;
using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Features.Surveys.SubmitSurveyResponse;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static System.Net.Mime.MediaTypeNames;

namespace ADAProjectAPIVerticalSlice.Tests.Features.Surveys.SubmitSurveyResponseTests;

//DER SKAL LAVES FLERE TEST METODER

[TestClass]
public class SubmitSurveyResponseHandlerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w =>
                w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    // Metode til at seede en test respondent i databasen
    private static async Task<(Guid AssessmentId, Guid RespondentId)> SeedRespondentAsync(
    ApplicationDbContext context,
    bool hasAnswered = false)
    {
        var assessment = new Assessment
        {
            AssessmentId = Guid.NewGuid(),
            AssessmentName = "Test Assessment",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            SurveyId = Guid.NewGuid()
        };

        var respondent = new Respondent
        {
            RespondentId = Guid.NewGuid(),
            AccessToken = "valid-token",
            HasAnswered = hasAnswered,
            AssessmentId = assessment.AssessmentId,
            Assessment = assessment
        };

        context.Assessments.Add(assessment);
        context.Respondents.Add(respondent);

        await context.SaveChangesAsync();

        return (assessment.AssessmentId, respondent.RespondentId);
    }

    [TestMethod]
    public async Task Handle_EmptyToken_ReturnsFailure()
    {
        // Arrange
        await using var context = CreateContext();
        var publisherMock = new Mock<IRabbitMqPublisher>();
        var handler = new Handler(context, publisherMock.Object);

        var command = new SubmitSurveyResponseCommand(
            Token: "",
            Answers: new()
            {
                new AnswerSubmission(Guid.NewGuid(), 4)
            },
            Comments: new(),
            RoleId: Guid.NewGuid(),
            RegionId: Guid.NewGuid(),
            ExperienceId: Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);

        publisherMock.Verify(
            p => p.PublishAsync(
                It.IsAny<ResponseSubmitted>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_NoAnswersProvided_ReturnsFailure()
    {
        // Arrange
        await using var context = CreateContext();
        var publisherMock = new Mock<IRabbitMqPublisher>();
        var handler = new Handler(context, publisherMock.Object);

        var command = new SubmitSurveyResponseCommand(
            Token: "valid-token",
            Answers: new(),
            Comments: new(),
            RoleId: Guid.NewGuid(),
            RegionId: Guid.NewGuid(),
            ExperienceId: Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);

        publisherMock.Verify(
            p => p.PublishAsync(
                It.IsAny<ResponseSubmitted>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_UnknownToken_ReturnsFailure()
    {
        // Arrange
        await using var context = CreateContext();
        var publisherMock = new Mock<IRabbitMqPublisher>();
        var handler = new Handler(context, publisherMock.Object);

        var command = new SubmitSurveyResponseCommand(
            Token: "unknown-token",
            Answers: new()
            {
                new AnswerSubmission(Guid.NewGuid(), 4)
            },
            Comments: new(),
            RoleId: Guid.NewGuid(),
            RegionId: Guid.NewGuid(),
            ExperienceId: Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);

        publisherMock.Verify(
            p => p.PublishAsync(
                It.IsAny<ResponseSubmitted>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_NullAnswers_ReturnsFailure()
    {
        // Arrange
        await using var context = CreateContext();
        var publisherMock = new Mock<IRabbitMqPublisher>();
        var handler = new Handler(context, publisherMock.Object);

        var command = new SubmitSurveyResponseCommand(
            Token: "valid-token",
            Answers: null!,
            Comments: new(),
            RoleId: Guid.NewGuid(),
            RegionId: Guid.NewGuid(),
            ExperienceId: Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);

        publisherMock.Verify(
            p => p.PublishAsync(
                It.IsAny<ResponseSubmitted>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_AlreadyAnswered_ReturnsFailure()
    {
        // Arrange
        await using var context = CreateContext();
        var publisher = new Mock<IRabbitMqPublisher>();

        await SeedRespondentAsync(context, hasAnswered: true);

        var handler = new Handler(context, publisher.Object);

        var command = new SubmitSurveyResponseCommand(
            Token: "valid-token",
            Answers: new()
            {
            new AnswerSubmission(Guid.NewGuid(), 4)
            },
            Comments: new(),
            RoleId: Guid.NewGuid(),
            RegionId: Guid.NewGuid(),
            ExperienceId: Guid.NewGuid());

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);

        publisher.Verify(
            p => p.PublishAsync(
                It.IsAny<ResponseSubmitted>(),
                "response-submitted",
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


[TestMethod]
public async Task Handle_InvalidRoleRegionExperience_ReturnsFailure()
    {
        // Arrange
        await using var context = CreateContext();
        var publisher = new Mock<IRabbitMqPublisher>();

        await SeedRespondentAsync(context);

        var handler = new Handler(context, publisher.Object);

        var command = new SubmitSurveyResponseCommand(
            Token: "valid-token",
            Answers: new()
            {
            new AnswerSubmission(Guid.NewGuid(), 4)
            },
            Comments: new(),
            RoleId: Guid.NewGuid(),
            RegionId: Guid.NewGuid(),
            ExperienceId: Guid.NewGuid());

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);

        publisher.Verify(
            p => p.PublishAsync(
                It.IsAny<ResponseSubmitted>(),
                "response-submitted",
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


}
