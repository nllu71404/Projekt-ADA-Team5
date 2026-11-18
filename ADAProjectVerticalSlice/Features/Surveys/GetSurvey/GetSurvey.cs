using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Features.Surveys.GetSurvey;

public class GetSurvey
{
    public record Query(string Token)
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
            // 1. Kontroller token
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return Result.Failure<GetSurveyResponse>(
                    Error.NullValue);
            }

            // 2. Find respondent og assessment
            var respondent = await _dbContext.Respondents
                .AsNoTracking()
                .Where(r => r.AccessToken == request.Token)
                .Select(r => new
                {
                    r.RespondentId,
                    r.AssessmentId,
                    r.HasAnswered,
                    r.Assessment.AssessmentName,
                    r.Assessment.StartDate,
                    r.Assessment.EndDate,
                    r.Assessment.SurveyId,
                    ApplicationName =
                        r.Assessment.Application.ApplicationName
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (respondent is null || respondent.HasAnswered)
            {
                return Result.Failure<GetSurveyResponse>(
                    Error.NullValue);
            }

            // 3. Kontroller om målingen er åben
            var now = DateTime.UtcNow;

            if (now < respondent.StartDate ||
                now > respondent.EndDate)
            {
                return Result.Failure<GetSurveyResponse>(
                    Error.NullValue);
            }

            // 4. Hent survey
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
                return Result.Failure<GetSurveyResponse>(
                    Error.NullValue);
            }

            // 5. Hent assessment med roller,
            // regioner og erfaringer
            var assessment = await _dbContext.Assessments
                .AsNoTracking()
                .AsSplitQuery()
                .Include(a => a.Roles)
                .Include(a => a.Regions)
                .Include(a => a.Experiences)
                .SingleAsync(
                    a => a.AssessmentId == respondent.AssessmentId,
                    cancellationToken
                );

            // 6. Hent temaer
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

            // 7. Hent spørgsmål
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

            // 8. Byg svarobjekt
            var response = new GetSurveyResponse
            {
                SurveyTitle = survey.Title,
                SurveyDescription = survey.Description,
                AssessmentName = respondent.AssessmentName,

                // HERE ARE YOUR THREE PROPERTIES

                Roles = assessment.Roles
                .Select(r => new SurveyRoleResponse
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .ToList(),

                        // Available regions for this assessment
                        Regions = assessment.Regions
                .Select(r => new SurveyRegionResponse
                {
                    RegionId = r.RegionId,
                    RegionName = r.RegionName
                })
                .ToList(),

                        // Available experience levels for this assessment
                        Experiences = assessment.Experiences
                .Select(e => new SurveyExperienceResponse
                {
                    ExperienceId = e.ExperienceId,
                    ExperienceValue = e.ExperienceValue
                })
                .ToList(),

                // Survey themes and questions
                Themes = themes.Select(theme =>
                    new SurveyThemeResponse
                    {
                        ThemeId = theme.ThemeId,
                        Title = theme.Title,
                        Description = theme.Description,

                        Questions = questions
                            .Where(q =>
                                q.ThemeId == theme.ThemeId)
                            .Select(q =>
                                new SurveyQuestionResponse
                                {
                                    QuestionId = q.QuestionId,

                                    QuestionText =
                                        q.QuestionText.Replace(
                                            "[System]",
                                            respondent.ApplicationName
                                        ),

                                    QuestionPolarity =
                                        q.QuestionPolarity,

                                    QuestionType =
                                        q.QuestionType,

                                    AnswerOptions =
                                        new List<int>
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

    // Carter endpoint
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/api/respondents/survey",
                async (
                    string token,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    var query = new Query(token);

                    var result = await sender.Send(query, ct);

                    if (result.IsFailure)
                    {
                        return Results.BadRequest(result.Error);
                    }

                    return Results.Ok(result.Value);
                })
                .AllowAnonymous();
        }
    }
}