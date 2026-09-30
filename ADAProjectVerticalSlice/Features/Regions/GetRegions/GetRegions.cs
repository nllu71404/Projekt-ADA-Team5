using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Features.Regions.GetRegions
{
    public class GetRegions
    {
        public record Query : IRequest<Result<List<Region>>>;

        public class Handler : IRequestHandler<Query, Result<List<Region>>>
        {
            private readonly ApplicationDbContext _dbContext;

            public Handler(ApplicationDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Result<List<Region>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var regions = await _dbContext.Regions
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return regions;
            }
        }

        public class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapGet(
                    "api/regions",
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
