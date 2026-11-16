using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Authentication;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Features.Authentication.GetCurrentUser
{
    public class GetCurrentUser
    {
        public class Query : IRequest<Result<GetCurrentUserResponse>>
        {
        }

        public class Handler : IRequestHandler<Query, Result<GetCurrentUserResponse>>
        {
            private readonly UserManager<User> _userManager;
            private readonly ICurrentUserService _currentUserService;
            private readonly ApplicationDbContext _dbContext;

            public Handler(
                UserManager<User> userManager,
                ICurrentUserService currentUserService,
                ApplicationDbContext dbContext)
            {
                _userManager = userManager;
                _currentUserService = currentUserService;
                _dbContext = dbContext;
            }

            public async Task<Result<GetCurrentUserResponse>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                // Hent id'en på den nuværende bruger fra ICurrentUserService
                var userId = _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Result.Failure<GetCurrentUserResponse>(Error.NullValue);
                }

                // Find brugeren i databasen ved hjælp af UserManager
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return Result.Failure<GetCurrentUserResponse>(
                        new Error(
                            "GetCurrentUser.NotFound",
                            "The current user could not be found."));
                }

                // hent brugerens roller
                var roles = await _userManager.GetRolesAsync(user);

                // hent brugerens company
                var company = await _dbContext.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == user.CompanyId, cancellationToken);

                // Opret en GetCurrentUserResponse med brugerens oplysninger
                var response = new GetCurrentUserResponse
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    JobTitle = user.JobTitle,
                    CompanyName = company?.CompanyName ?? string.Empty,
                    Roles = roles.ToList()
                };

                return response;
            }
        }

        public class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapGet(
                    "api/auth/me",
                    async (ISender sender) =>
                    {
                        var result = await sender.Send(new Query());

                        if (result.IsFailure)
                        {
                            if (result.Error.Code == "GetCurrentUser.NotFound")
                            {
                                return Results.NotFound(result.Error);
                            }

                            return Results.BadRequest(result.Error);
                        }

                        return Results.Ok(result.Value);
                    })
                    .RequireAuthorization()
                    .WithTags("Authentication");
            }
        }
    }
}