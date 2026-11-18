public class GetSurveyResponse
{
    public string SurveyTitle { get; set; } = string.Empty;
    public string SurveyDescription { get; set; } = string.Empty;
    public string AssessmentName { get; set; } = string.Empty;

    public List<SurveyThemeResponse> Themes { get; set; } = new();

    // Options the respondent can select
    public List<SurveyRoleOption> Roles { get; set; } = new();
    public List<SurveyRegionOption> Regions { get; set; } = new();
    public List<SurveyExperienceOption> Experiences { get; set; } = new();
}

public class SurveyThemeResponse
{
    public Guid ThemeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<SurveyQuestionResponse> Questions { get; set; } = new();
}

public class SurveyQuestionResponse
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionPolarity { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;

    public List<int> AnswerOptions { get; set; } =
        new() { 1, 2, 3, 4, 5 };
}

public record SurveyRoleOption(
    Guid RoleId,
    string RoleName
);

public record SurveyRegionOption(
    Guid RegionId,
    string RegionName
);

public record SurveyExperienceOption(
    Guid ExperienceId,
    string ExperienceValue
);