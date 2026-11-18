
using Carter;
using System.Net.Http.Json;
using System.Text.Json;

namespace ADAProjectAPIVerticalSlice.Features.Analysis;

public class GetAnalysis : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/analysis", async (
            IHttpClientFactory factory,
            ILogger<GetAnalysis> logger,
            CancellationToken ct) =>
        {
            var client = factory.CreateClient("PythonAnalysis");

            try
            {
                // 1. Request statistics from Python
                using var response = await client.GetAsync(
                    "/analysis",
                    ct
                );

                // 2. Check Python's response
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "Python analysis returned {StatusCode}",
                        response.StatusCode
                    );

                    return Results.Problem(
                        "Python analysis failed.",
                        statusCode: 502
                    );
                }

                // 3. Read statistics calculated by Python
                var result = await response.Content
                    .ReadFromJsonAsync<JsonElement>(
                        cancellationToken: ct
                    );

                // 4. Return statistics to the frontend
                return Results.Ok(result);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex,
                    "Python analysis service unavailable.");

                return Results.Problem(
                    "Python analysis service is unavailable.",
                    statusCode: 503
                );
            }
            catch (TaskCanceledException)
                when (!ct.IsCancellationRequested)
            {
                return Results.Problem(
                    "Python analysis timed out.",
                    statusCode: 504
                );
            }
        })
        .RequireAuthorization("AdminOnly");
    }
}
