using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Authentication;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using ADAProjectVerticalSlice.Features.Assessments.GetAssessments;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Features.Assessments.GetAssessments
{
    public class GetAssessments
    {
        // Query: Vi ønsker at hente assessments fra databasen.
        public class Query : IRequest<Result<List<GetAssessmentsResponse>>>
        {
        }

       

        // Handler: Henter data fra databasen.
        public class Handler : IRequestHandler<Query, Result<List<GetAssessmentsResponse>>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly ICurrentUserService _currentUserService;

            public Handler(
                ApplicationDbContext dbContext,
                ICurrentUserService currentUserService
                )
            {
                _dbContext = dbContext;
                _currentUserService = currentUserService;
            }

            public async Task<Result<List<GetAssessmentsResponse>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var userId = _currentUserService.UserId;

                var assessments = await _dbContext.Assessments
                    .AsNoTracking()
                    .Where(a => a.UserId == userId)
                    .Select(a => new GetAssessmentsResponse
                    {
                        AssessmentId = a.AssessmentId,
                        AssessmentName = a.AssessmentName,
                        ApplicationName = a.Application.ApplicationName,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate
                    })
                    .ToListAsync(cancellationToken);

                return assessments;
            }
        }

        // Endpoint: Gør det muligt for frontend at kalde query'en.
        public class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapGet(
                    "api/assessments",
                    async (ISender sender) =>
                    {
                        var result = await sender.Send(new Query());

                        if (result.IsFailure)
                            return Results.BadRequest(result.Error);

                        return Results.Ok(result.Value);
                    })
                    .RequireAuthorization("AdminOnly")
                    .WithTags("Assessments");
            }
        }
    }
}
