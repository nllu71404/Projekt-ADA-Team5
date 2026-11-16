using ADAProjectAPIVerticalSlice.Infrastructure.Authentication;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Features.Assessments.DeleteAssessment
{
    public class DeleteAssessment
    {
        public class Command : IRequest<Result<Guid>>
        {
            public Guid AssessmentId { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly ICurrentUserService _currentUserService;

            public Handler(
                ApplicationDbContext dbContext,
                ICurrentUserService currentUserService)
            {
                _dbContext = dbContext;
                _currentUserService = currentUserService;
            }

            public async Task<Result<Guid>> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var userId = _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Result.Failure<Guid>(Error.NullValue);
                }

                // Find the assessment belonging to the logged-in user.
                var assessment = await _dbContext.Assessments
                    .Include(a => a.Respondents)
                    .FirstOrDefaultAsync(
                        a => a.AssessmentId == request.AssessmentId
                             && a.UserId == userId,
                        cancellationToken);

                if (assessment == null)
                {
                    return Result.Failure<Guid>(
                        new Error(
                            "DeleteAssessment.NotFound",
                            "The assessment could not be found."));
                }

                // Check whether any respondent has already answered.
                var hasAnswered = assessment.Respondents
                    .Any(r => r.HasAnswered);

                // Also check the actual Answer records in the database.
                var respondentIds = assessment.Respondents
                    .Select(r => r.RespondentId)
                    .ToList();

                var hasAnswerRecords = await _dbContext.Answers
                    .AnyAsync(
                        a => respondentIds.Contains(a.RespondentId),
                        cancellationToken);

                if (hasAnswered || hasAnswerRecords)
                {
                    return Result.Failure<Guid>(
                        new Error(
                            "DeleteAssessment.HasAnswers",
                            "This assessment cannot be deleted because respondents have already submitted answers."));
                }

                // Delete the assessment.
                // Respondents are deleted through the configured cascade relationship.
                _dbContext.Assessments.Remove(assessment);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return assessment.AssessmentId;
            }
        }

        public class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapDelete(
                    "api/assessments/{assessmentId:guid}",
                    async (Guid assessmentId, ISender sender) =>
                    {
                        var result = await sender.Send(
                            new Command { AssessmentId = assessmentId });

                        if (result.IsFailure)
                        {
                            if (result.Error.Code == "DeleteAssessment.NotFound")
                            {
                                return Results.NotFound(result.Error);
                            }

                            return Results.Conflict(result.Error);
                        }

                        return Results.Ok(result.Value);
                    })
                    .RequireAuthorization("AdminOnly")
                    .WithTags("Assessments");
            }
        }
    }
}