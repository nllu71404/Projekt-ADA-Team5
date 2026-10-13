using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Features.Surveys.SubmitSurveyResponse;

// Command
public record SubmitSurveyResponseCommand(
    string Token,
    List<AnswerSubmission> Answers,
    List<CommentSubmission> Comments)
    : IRequest<Result>;

// Handler
public class Handler : IRequestHandler<SubmitSurveyResponseCommand, Result>
{
    private readonly ApplicationDbContext _dbContext;

    public Handler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        SubmitSurveyResponseCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Valider respondentens token
        if (string.IsNullOrWhiteSpace(request.Token))
            return Result.Failure(Error.InvalidRespondentToken);

        // 2. Valider de indsendte svar
        if (request.Answers is null || request.Answers.Count == 0)
            return Result.Failure(Error.NoAnswersProvided);

        // 3. Find respondenten og den tilknyttede assessment
        var respondent = await _dbContext.Respondents
            .Include(r => r.Assessment)
            .SingleOrDefaultAsync(
                r => r.AccessToken == request.Token,
                cancellationToken);

        if (respondent is null)
            return Result.Failure(Error.InvalidRespondentToken);

        if (respondent.HasAnswered)
            return Result.Failure(Error.SurveyAlreadyAnswered);

        // 4. Tjek om surveyen er åben for besvarelse
        var now = DateTime.UtcNow;

        if (now < respondent.Assessment.StartDate ||
            now > respondent.Assessment.EndDate)
        {
            return Result.Failure(Error.SurveyNotOpen);
        }

        // 5. Hent alle spørgsmål for den tilknyttede survey
        var surveyId = respondent.Assessment.SurveyId;

        var questions = await _dbContext.Questions
            .AsNoTracking()
            .Where(q => q.SurveyId == surveyId)
            .Select(q => new
            {
                q.QuestionId,
                q.ThemeId
            })
            .ToListAsync(cancellationToken);

        var questionIds = questions
            .Select(q => q.QuestionId)
            .ToHashSet();

        // 6. Tjek at alle indsendte svar refererer til gyldige spørgsmål
        if (request.Answers.Any(a => !questionIds.Contains(a.QuestionId)))
            return Result.Failure(Error.InvalidQuestion);

        // 7. Modtag kun ét svar per spørgsmål
        if (request.Answers
            .Select(a => a.QuestionId)
            .Distinct()
            .Count() != request.Answers.Count)
        {
            return Result.Failure(Error.DuplicateAnswer);
        }

        // 8. Valider at alle point er mellem 1 og 5
        if (request.Answers.Any(a => a.Point < 1 || a.Point > 5))
            return Result.Failure(Error.InvalidAnswerPoint);

        // 9. Der skal være et svar for hvert spørgsmål
        if (request.Answers.Count != questions.Count)
            return Result.Failure(Error.IncompleteAnswers);

        // 10. Valider kommentarer
        var themeIds = questions
            .Select(q => q.ThemeId)
            .ToHashSet();

        if (request.Comments is null)
            return Result.Failure(Error.ConditionNotMet);

        if (request.Comments.Any(c => !themeIds.Contains(c.ThemeId)))
            return Result.Failure(Error.InvalidCommentTheme);

        // En kommentar per tema, hvis der er kommentarer
        if (request.Comments
            .Select(c => c.ThemeId)
            .Distinct()
            .Count() != request.Comments.Count)
        {
            return Result.Failure(Error.DuplicateComment);
        }

        // 11. Opret Answer entities
        var answers = request.Answers.Select(a => new Answer
        {
            AnswerId = Guid.NewGuid(),
            QuestionId = a.QuestionId,
            RespondentId = respondent.RespondentId,
            Point = a.Point
        }).ToList();

        // 12. Opret Comment entities
        // Tomme kommentarer ignoreres
        var comments = request.Comments
            .Where(c => !string.IsNullOrWhiteSpace(c.FreeForm))
            .Select(c => new Comment
            {
                CommentId = Guid.NewGuid(),
                ThemeId = c.ThemeId,
                RespondentId = respondent.RespondentId,
                FreeForm = c.FreeForm.Trim()
            })
            .ToList();

        // 13. Gem svar og kommentarer i databasen inden for en transaktion
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        _dbContext.Answers.AddRange(answers);
        _dbContext.Comments.AddRange(comments);

        respondent.HasAnswered = true;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}

// Endpoint
public class Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "api/respondents/survey/submit",
            async (
                SubmitSurveyResponseRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new SubmitSurveyResponseCommand(
                    request.Token,
                    request.Answers,
                    request.Comments);

                var result = await sender.Send(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.NoContent();
            });
    }
}
