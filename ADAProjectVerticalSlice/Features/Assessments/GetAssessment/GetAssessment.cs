using ADAProjectAPIVerticalSlice.Infrastructure.Authentication;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Features.Assessments.GetAssessment
{
    public class GetAssessment
    {
        // QUERY
        public class Query : IRequest<Result<GetAssessmentResponse>>
        {
            public Guid AssessmentId { get; set; }
        }

        // HANDLER
        public class Handler : IRequestHandler<Query, Result<GetAssessmentResponse>>
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

            public async Task<Result<GetAssessmentResponse>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                // Hent den aktuelle bruger.
                var userId = _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Result.Failure<GetAssessmentResponse>(
                        Error.NullValue);
                }

                // Hent vurderingen, hvis den tilhører brugeren.
                var assessment = await _dbContext.Assessments
                    .AsNoTracking()
                    .Where(a =>
                        a.AssessmentId == request.AssessmentId &&
                        a.UserId == userId)
                    .Select(a => new GetAssessmentResponse
                    {
                        AssessmentId = a.AssessmentId,
                        AssessmentName = a.AssessmentName,
                        ApplicationName = a.Application.ApplicationName,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,

                        RoleNames = a.Roles
                            .Select(r => r.RoleName)
                            .ToList(),

                        RegionIds = a.Regions
                            .Select(r => r.RegionId)
                            .ToList(),

                        ExperienceIds = a.Experiences
                            .Select(e => e.ExperienceId)
                            .ToList(),

                        RespondentEmails = a.Respondents
                            .Select(r => r.EmailAddress)
                            .ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (assessment is null)
                {
                    return Result.Failure<GetAssessmentResponse>(
                        new Error(
                            "GetAssessment.NotFound",
                            "Vurderingen kunne ikke findes."));
                }

                return assessment;
            }
        }

        // ENDPOINT
        public class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapGet(
                    "api/assessments/{assessmentId:guid}",
                    async (Guid assessmentId, ISender sender) =>
                    {
                        var result = await sender.Send(
                            new Query
                            {
                                AssessmentId = assessmentId
                            });

                        if (result.IsFailure)
                        {
                            return Results.BadRequest(result.Error);
                        }

                        return Results.Ok(result.Value);
                    })
                    .RequireAuthorization("AdminOnly")
                    .WithTags("Assessments");
            }
        }
    }
}