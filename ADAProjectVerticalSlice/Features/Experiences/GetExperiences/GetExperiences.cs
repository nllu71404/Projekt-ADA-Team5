using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;

namespace ADAProjectAPIVerticalSlice.Features.Experiences.GetExperiences
{
    public class GetExperiences
    {
        public record Query : IRequest<Result<List<Experience>>>;

        public class Handler : IRequestHandler<Query, Result<List<Experience>>>
        {
            private readonly ApplicationDbContext _dbContext;

            public Handler(ApplicationDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Result<List<Experience>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var experiences = await _dbContext.Experiences
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return experiences;
            }
        }

        public class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapGet(
                    "api/experiences",
                    async (ISender sender) =>
                    {
                        var result = await sender.Send(new Query());

                        if (result.IsFailure)
                        {
                            return Results.BadRequest(result.Error);
                        }

                        return Results.Ok(result.Value);
                    });
            }
        }
    }
}
