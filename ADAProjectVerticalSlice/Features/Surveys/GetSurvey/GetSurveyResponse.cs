public class GetSurveyResponse
{
    public string SurveyTitle { get; set; } = string.Empty;
    public string SurveyDescription { get; set; } = string.Empty;
    public string AssessmentName { get; set; } = string.Empty;

    // NYT: det, respondenten kan vælge imellem (det, målingen blev oprettet med)
    public List<SurveyRoleResponse> Roles { get; set; } = new();
    public List<SurveyRegionResponse> Regions { get; set; } = new();
    public List<SurveyExperienceResponse> Experiences { get; set; } = new();

    public List<SurveyThemeResponse> Themes { get; set; } = new();
}

// NYT
public class SurveyRoleResponse
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

// NYT
public class SurveyRegionResponse
{
    public Guid RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
}

// NYT
public class SurveyExperienceResponse
{
    public Guid ExperienceId { get; set; }
    public string ExperienceValue { get; set; } = string.Empty;
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

    public List<int> AnswerOptions { get; set; } = new() { 1, 2, 3, 4, 5 };
}
