
namespace ADA.Frontend.Services;

// Assessment dropdown
public class SurveyAssessmentDto
{
    public Guid AssessmentId { get; set; }
    public string AssessmentName { get; set; } = "";
    public string ApplicationName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

// Response from Python
public class SurveyStatisticsDto
{
    public Guid AssessmentId { get; set; }
    public int CompletedRespondents { get; set; }
    public string? Message { get; set; }

    public SurveyScoreDto? Overall { get; set; }

    public List<SurveyThemeStatisticsDto> Themes { get; set; } = new();
    public List<SurveyQuestionStatisticsDto> Questions { get; set; } = new();

    public List<SurveyGroupStatisticsDto> ByRole { get; set; } = new();
    public List<SurveyGroupStatisticsDto> ByRegion { get; set; } = new();
    public List<SurveyGroupStatisticsDto> ByExperience { get; set; } = new();
}

public class SurveyScoreDto
{
    public int AnswerCount { get; set; }
    public int RespondentCount { get; set; }

    public double AverageScore { get; set; }
    public double MedianScore { get; set; }
    public double? StandardDeviation { get; set; }
    public double FavorablePercent { get; set; }

    public List<SurveyScoreDistributionDto> Distribution { get; set; } = new();
}

public class SurveyScoreDistributionDto
{
    public int Score { get; set; }
    public int Count { get; set; }
}

public class SurveyThemeStatisticsDto
{
    public Guid ThemeId { get; set; }
    public string Title { get; set; } = "";
    public SurveyScoreDto Statistics { get; set; } = new();
}

public class SurveyQuestionStatisticsDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = "";
    public SurveyScoreDto Statistics { get; set; } = new();
}

public class SurveyGroupStatisticsDto
{
    public string Name { get; set; } = "";
    public SurveyScoreDto Statistics { get; set; } = new();
}
