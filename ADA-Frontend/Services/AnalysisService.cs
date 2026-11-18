
using System.Net;
using System.Net.Http.Json;

namespace ADA.Frontend.Services;

// The results calculated by Python


public sealed record AssessmentMonth(
    string Month,
    int Count
);

public sealed record AssessmentResponseSummary(
    Guid AssessmentId,
    string AssessmentName,
    int TotalRespondents,
    int AnsweredRespondents,
    double ResponseRatePercent
);

public sealed record AnalysisResult(
    int TotalAssessments,
    int ActiveAssessments,
    int UpcomingAssessments,
    int PastAssessments,
    double AverageDurationDays,
    List<AssessmentMonth> AssessmentsByMonth,

    int TotalRespondents,
    int AnsweredRespondents,
    int UnansweredRespondents,
    double ResponseRatePercent,
    List<AssessmentResponseSummary> ResponsesByAssessment
);


public class AnalysisService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthSession _authSession;

    public AnalysisService(
        IHttpClientFactory httpClientFactory,
        AuthSession authSession)
    {
        _httpClientFactory = httpClientFactory;
        _authSession = authSession;
    }

    public async Task<AnalysisResult?> GetAnalysisAsync()
    {
        if (!_authSession.IsAuthenticated)
        {
            throw new InvalidOperationException(
                "Please log in first.");
        }

        // Use your existing configured API client
        using var client =
            _httpClientFactory.CreateClient("ADAApi");

        // Call the C# API, NOT Python directly
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "api/analysis"
        );

        // Attach your existing authentication cookie
        if (!string.IsNullOrWhiteSpace(
            _authSession.CookieHeader))
        {
            request.Headers.TryAddWithoutValidation(
                "Cookie",
                _authSession.CookieHeader
            );
        }

        using var response =
            await client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "Your session has expired. Please log in again.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException(
                "You need the Admin role to view analysis.");
        }

        response.EnsureSuccessStatusCode();

        // Read the JSON results produced by Python
        return await response.Content
            .ReadFromJsonAsync<AnalysisResult>();
    }
}
