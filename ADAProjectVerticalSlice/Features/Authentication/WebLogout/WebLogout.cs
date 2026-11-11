using Carter;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ADAProjectAPIVerticalSlice.Features.Authentication.WebLogout
{
    public class WebLogout : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/api/auth/web-logout",
                async (HttpContext httpContext) =>
                {
                    await httpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    return Results.Ok(new
                    {
                        message = "Logout successful"
                    });
                })
                .RequireAuthorization()
                .WithTags("Authentication");
        }
    }
}
