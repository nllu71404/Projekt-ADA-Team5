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
                    Console.WriteLine(
    $"IsAuthenticated: {httpContext.User.Identity?.IsAuthenticated}");

                    Console.WriteLine(
                        $"AuthenticationType: {httpContext.User.Identity?.AuthenticationType}");

                    Console.WriteLine(
                        $"Cookie exists: {httpContext.Request.Cookies.ContainsKey("ADA.Auth")}");
                    await httpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    return Results.Redirect("http://localhost:5026/login");
                })
                .RequireAuthorization()
                .WithTags("Authentication");
        }
    }
}
