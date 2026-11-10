using Carter;
using Microsoft.AspNetCore.Antiforgery;

namespace ADAProjectAPIVerticalSlice.Features.Authentication.GetAntiforgeryToken
{
    public class GetAntiforgeryToken : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/api/auth/csrf-token",
                (IAntiforgery antiforgery, HttpContext httpContext) =>
                {
                    var tokens = antiforgery.GetAndStoreTokens(httpContext);

                    return Results.Ok(new
                    {
                        requestToken = tokens.RequestToken
                    });
                })
                .AllowAnonymous()
                .WithTags("Authentication");
        }
    }
}
