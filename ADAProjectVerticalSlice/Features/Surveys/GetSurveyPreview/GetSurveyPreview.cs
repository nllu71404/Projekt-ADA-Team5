using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using ADAProjectAPIVerticalSlice.Features.Surveys.GetSurvey;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Features.Surveys.GetSurveyPreview;

public class GetSurveyPreview
{
    // Den faste ADA Survey
    private static readonly Guid SurveyId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    public record Query()
        : IRequest<Result<GetSurveyResponse>>;

    public class Handler
        : IRequestHandler<Query, Result<GetSurveyResponse>>
    {
        private readonly ApplicationDbContext _dbContext;

        public Handler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<GetSurveyResponse>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            // 1. Hent den faste survey
            var survey = await _dbContext.Surveys
                .AsNoTracking()
                .Where(s => s.SurveyId == SurveyId)
                .Select(s => new
                {
                    s.Title,
                    s.Description
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (survey is null)
            {
                return Result.Failure<GetSurveyResponse>(
                    Error.NullValue);
            }

            // 2. Hent surveyens temaer
            var themes = await _dbContext.Themes
                .AsNoTracking()
                .Where(t => t.SurveyId == SurveyId)
                .Select(t => new
                {
                    t.ThemeId,
                    t.Title,
                    t.Description
                })
                .ToListAsync(cancellationToken);

            // 3. Hent surveyens spørgsmål
            var questions = await _dbContext.Questions
                .AsNoTracking()
                .Where(q => q.SurveyId == SurveyId)
                .Select(q => new
                {
                    q.QuestionId,
                    q.ThemeId,
                    q.QuestionText,
                    q.QuestionPolarity,
                    q.QuestionType
                })
                .ToListAsync(cancellationToken);

            // 4. Byg preview-response
            var response = new GetSurveyResponse
            {
                SurveyTitle = survey.Title,
                SurveyDescription = survey.Description,

                // Der er ingen assessment i et preview
                AssessmentName = "Survey preview",

                // Ingen respondentvalg i preview
                Roles = new(),
                Regions = new(),
                Experiences = new(),

                // Temaer og spørgsmål
                Themes = themes.Select(theme =>
                    new SurveyThemeResponse
                    {
                        ThemeId = theme.ThemeId,
                        Title = theme.Title,
                        Description = theme.Description,

                        Questions = questions
                            .Where(q => q.ThemeId == theme.ThemeId)
                            .Select(q => new SurveyQuestionResponse
                            {
                                QuestionId = q.QuestionId,
                                QuestionText = q.QuestionText,
                                QuestionPolarity = q.QuestionPolarity,
                                QuestionType = q.QuestionType,

                                AnswerOptions = new List<int>
                                {
                                    1, 2, 3, 4, 5
                                }
                            })
                            .ToList()
                    })
                    .ToList()
            };

            return response;
        }
    }

    // Endpoint
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/api/surveys/preview",
                async (
                    ISender sender,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(
                        new Query(),
                        ct);

                    if (result.IsFailure)
                    {
                        return Results.NotFound(result.Error);
                    }

                    return Results.Ok(result.Value);
                })
                .RequireAuthorization("UserOrAdmin")
                .WithTags("Surveys");
        }
    }
}
