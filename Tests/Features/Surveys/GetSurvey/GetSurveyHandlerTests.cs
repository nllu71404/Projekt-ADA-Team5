
using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Features.Surveys.GetSurvey;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADAProjectAPIVerticalSlice.Tests.Features.Surveys.GetSurveyTests;

[TestClass]
public class GetSurveyHandlerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    // Seed databasen med en åpen survey, assessment, respondent, theme og question
    private static async Task SeedOpenSurveyAsync(
        ApplicationDbContext context,
        bool hasAnswered = false,
        bool includeSurvey = true)
    {
        var surveyId = Guid.NewGuid();
        var assessmentId = Guid.NewGuid();
        var respondentId = Guid.NewGuid();

        var survey = new Survey
        {
            SurveyId = surveyId,
            Title = "ADA Survey",
            Description = "Survey description"
        };

        var assessment = new Assessment
        {
            AssessmentId = assessmentId,
            AssessmentName = "ADA Measurement",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            SurveyId = surveyId,
            Survey = survey
        };

        var respondent = new Respondent
        {
            RespondentId = respondentId,
            AccessToken = "valid-token",
            HasAnswered = hasAnswered,
            AssessmentId = assessmentId,
            Assessment = assessment
        };

        var theme = new Theme
        {
            ThemeId = Guid.NewGuid(),
            Title = "Collaboration",
            Description = "Collaboration description",
            SurveyId = surveyId,
            Survey = survey
        };

        var question = new Question
        {
            QuestionId = Guid.NewGuid(),
            QuestionText = "We collaborate effectively",
            QuestionPolarity = "Positive",
            QuestionType = "Scale",
            SurveyId = surveyId,
            ThemeId = theme.ThemeId,
            Survey = survey,
            Theme = theme
        };

        // Survey er valgfri, så vi tilføjer den kun hvis includeSurvey er true
        if (includeSurvey)
        {
            context.Surveys.Add(survey);

            assessment.Survey = survey;
            theme.Survey = survey;
            question.Survey = survey;
        }
        else
        {
            assessment.Survey = null!;
            theme.Survey = null!;
            question.Survey = null!;
        }

        context.Assessments.Add(assessment);
        context.Respondents.Add(respondent);
        context.Themes.Add(theme);
        context.Questions.Add(question);

        await context.SaveChangesAsync();
    }

    [TestMethod]
    public async Task Handle_ValidTokenAndOpenAssessment_ReturnsSurvey()
    {
        // Arrange
        await using var context = CreateContext();
        await SeedOpenSurveyAsync(context);

        var handler = new GetSurvey.Handler(context);
        var query = new GetSurvey.Query("valid-token");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("ADA Survey", result.Value.SurveyTitle);
        Assert.AreEqual("ADA Measurement", result.Value.AssessmentName);
        Assert.AreEqual(1, result.Value.Themes.Count);
        Assert.AreEqual(1, result.Value.Themes[0].Questions.Count);
        Assert.AreEqual(
            "We collaborate effectively",
            result.Value.Themes[0].Questions[0].QuestionText);
        CollectionAssert.AreEqual(
            new[] { 1, 2, 3, 4, 5 },
            result.Value.Themes[0].Questions[0].AnswerOptions);
    }

    [TestMethod]
    public async Task Handle_UnknownToken_ReturnsFailure()
    {
        // Arrange
        await using var context = CreateContext();
        await SeedOpenSurveyAsync(context);

        var handler = new GetSurvey.Handler(context);
        var query = new GetSurvey.Query("unknown-token");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);
    }

    [TestMethod]
    public async Task Handle_RespondentAlreadyAnswered_ReturnsFailure()
    {
        // Arrange
        await using var context = CreateContext();
        await SeedOpenSurveyAsync(context, hasAnswered: true);

        var handler = new GetSurvey.Handler(context);
        var query = new GetSurvey.Query("valid-token");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);
    }

    [TestMethod]
    public async Task Handle_AssessmentNotStarted_ReturnsFailure()
    {
        // Arrange
        await using var context = CreateContext();
        await SeedOpenSurveyAsync(context);

        var assessment = await context.Assessments.SingleAsync();
        assessment.StartDate = DateTime.UtcNow.AddDays(1);
        assessment.EndDate = DateTime.UtcNow.AddDays(2);
        await context.SaveChangesAsync();

        var handler = new GetSurvey.Handler(context);
        var query = new GetSurvey.Query("valid-token");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);
    }

    [TestMethod]
    public async Task Handle_AssessmentEnded_ReturnsFailure()
    {
        // Arrange
        await using var context = CreateContext();
        await SeedOpenSurveyAsync(context);

        var assessment = await context.Assessments.SingleAsync();
        assessment.StartDate = DateTime.UtcNow.AddDays(-2);
        assessment.EndDate = DateTime.UtcNow.AddDays(-1);
        await context.SaveChangesAsync();

        var handler = new GetSurvey.Handler(context);
        var query = new GetSurvey.Query("valid-token");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);
    }

    [TestMethod]
    public async Task Handle_SurveyMissing_ReturnsFailure()
    {
        // Arrange
        await using var context = CreateContext();
        await SeedOpenSurveyAsync(context, includeSurvey: false);

        var handler = new GetSurvey.Handler(context);
        var query = new GetSurvey.Query("valid-token");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);
    }
}

