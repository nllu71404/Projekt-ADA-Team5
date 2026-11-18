
using Carter;
using ADAProjectAPIVerticalSlice.Infrastructure.Authentication;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json;

namespace ADAProjectAPIVerticalSlice.Features.Analysis;

public class GetAssessmentSurveyAnalysis : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/analysis/assessment/{assessmentId:guid}",
            async (
                Guid assessmentId,
                ICurrentUserService currentUser,
                ApplicationDbContext db,
                IHttpClientFactory factory,
                CancellationToken ct) =>
            {
                var userId = currentUser.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Unauthorized();

                // Match the ownership rule used by GetAssessment
                var authorized = await db.Assessments
                    .AsNoTracking()
                    .AnyAsync(a =>
                        a.AssessmentId == assessmentId &&
                        a.UserId == userId, ct);

                if (!authorized)
                    return Results.NotFound();

                var client =
                    factory.CreateClient("PythonAnalysis");

                try
                {
                    using var response = await client.GetAsync(
                        $"/analysis/assessment/{assessmentId}",
                        ct
                    );

                    if (!response.IsSuccessStatusCode)
                        return Results.Problem(
                            "Python survey analysis failed.",
                            statusCode: 502
                        );

                    var result = await response.Content
                        .ReadFromJsonAsync<JsonElement>(
                            cancellationToken: ct
                        );

                    return Results.Ok(result);
                }
                catch (HttpRequestException)
                {
                    return Results.Problem(
                        "Python service unavailable.",
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
