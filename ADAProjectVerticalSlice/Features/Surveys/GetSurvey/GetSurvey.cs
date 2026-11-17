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
                   ApplicationName = r.Assessment.Application.ApplicationName,

                   // NYT: det, respondenten kan vælge imellem. Det er MÅLINGENS roller, regioner og erfaringer
                   // (r.Assessment.Roles osv.), altså dem, produktejeren oprettede målingen med. EF henter dem fra
                   // tabellerne AssessmentRole, AssessmentRegion og AssessmentExperience. Det er ikke noget, der
                   // ligger på respondenten selv (Respondent.RoleId osv. bliver først sat, når svaret sendes).
                   AssessmentRoles = r.Assessment.Roles
                       .OrderBy(x => x.RoleName)
                       .Select(x => new { x.RoleId, x.RoleName })
                       .ToList(),
                   AssessmentRegions = r.Assessment.Regions
                       .OrderBy(x => x.RegionName)
                       .Select(x => new { x.RegionId, x.RegionName })
                       .ToList(),
                   AssessmentExperiences = r.Assessment.Experiences
                       .Select(x => new { x.ExperienceId, x.ExperienceValue })
                       .ToList()
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

                // NYT: målingens valgmuligheder til siden "Om dig" i appen.
                Roles = respondent.AssessmentRoles
                    .Select(x => new SurveyRoleResponse { RoleId = x.RoleId, RoleName = x.RoleName })
                    .ToList(),
                Regions = respondent.AssessmentRegions
                    .Select(x => new SurveyRegionResponse { RegionId = x.RegionId, RegionName = x.RegionName })
                    .ToList(),
                Experiences = respondent.AssessmentExperiences
                    .Select(x => new SurveyExperienceResponse
                    {
                        ExperienceId = x.ExperienceId,
                        ExperienceValue = x.ExperienceValue
                    })
                    .ToList(),

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
                })
            .RequireAuthorization("UserOrAdmin");
        }

    }
}
