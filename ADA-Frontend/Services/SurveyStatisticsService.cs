
using System.Net;
using System.Net.Http.Json;

namespace ADA.Frontend.Services;

public class SurveyStatisticsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthSession _authSession;

    public SurveyStatisticsService(
        IHttpClientFactory httpClientFactory,
        AuthSession authSession)
    {
        _httpClientFactory = httpClientFactory;
        _authSession = authSession;
    }

    // Retrieve assessments owned by the logged-in user
    public async Task<List<SurveyAssessmentDto>> GetAssessmentsAsync()
    {
        return await GetAsync<List<SurveyAssessmentDto>>(
            "api/assessments"
        ) ?? new List<SurveyAssessmentDto>();
    }

    // Retrieve Python statistics for a specific assessment
    public async Task<SurveyStatisticsDto?> GetStatisticsAsync(
        Guid assessmentId)
    {
        return await GetAsync<SurveyStatisticsDto>(
            $"api/analysis/assessment/{assessmentId}"
        );
    }

    private async Task<T?> GetAsync<T>(string endpoint)
    {
        if (!_authSession.IsAuthenticated)
        {
            throw new InvalidOperationException(
                "Please log in first.");
        }

        using var client =
            _httpClientFactory.CreateClient("ADAApi");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            endpoint
        );

        // Attach authentication cookie
        if (!string.IsNullOrWhiteSpace(_authSession.CookieHeader))
        {
            request.Headers.TryAddWithoutValidation(
                "Cookie",
                _authSession.CookieHeader
            );
        }

        using var response = await client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "Your session has expired. Please log in again.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException(
                "You need Admin permission to view statistics.");
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>();
    }
}
