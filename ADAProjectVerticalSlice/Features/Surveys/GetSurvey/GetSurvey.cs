using ADAProjectAPIVerticalSlice.Features.Surveys.GetSurvey;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

// Tilpas namespace/imports for Result og Error til dit projekt.

namespace ADAProjectAPIVerticalSlice.Features.Surveys.GetSurvey;

public class GetSurvey
{
    public record Query(string Token) : IRequest<Result<GetSurveyResponse>>;

    public class Handler : IRequestHandler<Query, Result<GetSurveyResponse>>
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
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return Result.Failure<GetSurveyResponse>(Error.NullValue);
            }

            // 1. Find respondenten og den tilhørende måling.
            var respondent = await _dbContext.Respondents
                .AsNoTracking()
                .Where(r => r.AccessToken == request.Token)
               .Select(r => new
               {
                   r.RespondentId,
                   r.HasAnswered,
                   r.Assessment.AssessmentName,
                   r.Assessment.StartDate,
                   r.Assessment.EndDate,
                   r.Assessment.SurveyId,
                   ApplicationName = r.Assessment.Application.ApplicationName
               })
                .SingleOrDefaultAsync(cancellationToken);

            if (respondent is null || respondent.HasAnswered)
            {
                return Result.Failure<GetSurveyResponse>(Error.NullValue);
            }

            // 2. Kontrollér, at målingen er åben.
            var now = DateTime.UtcNow;

            if (now < respondent.StartDate || now > respondent.EndDate)
            {
                return Result.Failure<GetSurveyResponse>(Error.NullValue);
            }

            // 3. Hent surveyens titel og beskrivelse.
            var survey = await _dbContext.Surveys
                .AsNoTracking()
                .Where(s => s.SurveyId == respondent.SurveyId)
                .Select(s => new
                {
                    s.Title,
                    s.Description
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (survey is null)
            {
                return Result.Failure<GetSurveyResponse>(Error.NullValue);
            }

            // 4. Hent temaer og spørgsmål separat.
            // Det kræver ikke navigation properties på Survey eller Theme.
            var themes = await _dbContext.Themes
                .AsNoTracking()
                .Where(t => t.SurveyId == respondent.SurveyId)
                .Select(t => new
                {
                    t.ThemeId,
                    t.Title,
                    t.Description
                })
                .ToListAsync(cancellationToken);

            var questions = await _dbContext.Questions
                .AsNoTracking()
                .Where(q => q.SurveyId == respondent.SurveyId)
                .Select(q => new
                {
                    q.QuestionId,
                    q.ThemeId,
                    q.QuestionText,
                    q.QuestionPolarity,
                    q.QuestionType
                })
                .ToListAsync(cancellationToken);

            // 5. Saml spørgsmålene under de rigtige temaer.
            var response = new GetSurveyResponse
            {
                SurveyTitle = survey.Title,
                SurveyDescription = survey.Description,
                AssessmentName = respondent.AssessmentName,

                Themes = themes.Select(theme => new SurveyThemeResponse
                {
                    ThemeId = theme.ThemeId,
                    Title = theme.Title,
                    Description = theme.Description,

                    Questions = questions
                        .Where(q => q.ThemeId == theme.ThemeId)
                        .Select(q => new SurveyQuestionResponse
                        {
                            QuestionId = q.QuestionId,
                            QuestionText = q.QuestionText.Replace(
                            "[System]",
                            respondent.ApplicationName),
                            QuestionPolarity = q.QuestionPolarity,
                            QuestionType = q.QuestionType,
                            AnswerOptions = new List<int> { 1, 2, 3, 4, 5 }
                        })
                        .ToList()
                }).ToList()
            };

            return response;
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // HTTP GET
            app.MapGet(
                "api/respondents/survey",
                async (string token, ISender sender) =>
                {
                    // Opretter Query'en med respondentens adgangstoken.
                    var query = new Query(token);

                    // Sender Query'en til MediatR, som finder den korrekte Handler.
                    var result = await sender.Send(query);

                    if (result.IsFailure)
                    {
                        return Results.BadRequest(result.Error);
                    }

                    return Results.Ok(result.Value);
                });
        }
    }
}
