using ADAProjectAPIVerticalSlice.Features.Respondents.ExchangeMagicLink;
using ADAProjectAPIVerticalSlice.Infrastructure.Authentication;
using Carter;
using MediatR;

namespace ADAProjectVerticalSlice.Features.Respondents.ExchangeMagicLinkMobile;

public class Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/respondents/exchange-magic-link/mobile",
            async (
                ExchangeMagicLink.Command request,
                ISender sender,
                IJwtTokenService jwtTokenService,
                CancellationToken ct) =>
            {
                // 1. Genbrug den eksisterende magic link-handler.
                var result = await sender.Send(request, ct);

                if (result.IsFailure)
                {
                    if (result.Error.Code == "ExchangeMagicLink.Validation")
                    {
                        return Results.BadRequest(new
                        {
                            error = result.Error.Message
                        });
                    }

                    if (result.Error.Code == "ExchangeMagicLink.InvalidToken")
                    {
                        return Results.Json(
                            new { error = result.Error.Message },
                            statusCode: StatusCodes.Status401Unauthorized);
                    }

                    if (result.Error.Code == "ExchangeMagicLink.AlreadyAnswered" ||
                        result.Error.Code == "ExchangeMagicLink.SurveyNotOpen")
                    {
                        return Results.BadRequest(new
                        {
                            error = result.Error.Message
                        });
                    }

                    return Results.BadRequest(result.Error);
                }

                // 2. Hent respondentens claims.
                var respondentId = result.Value!
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!
                    .Value;

                var email = result.Value!
                    .FindFirst(System.Security.Claims.ClaimTypes.Email)!
                    .Value;

                // 3. Generer JWT til mobil.
                var token = await jwtTokenService.CreateRespondentTokenAsync(
                    respondentId,
                    email);

                // 4. Returner JWT til mobil.
                return Results.Ok(new
                {
                    accessToken = token,
                    tokenType = "Bearer"
                });
            })
            .AllowAnonymous()
            .WithTags("Respondents");
    }
}
