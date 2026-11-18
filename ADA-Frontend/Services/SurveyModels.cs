namespace ADA.Frontend.Services;

public sealed class SurveyResponseDto
{
    public string SurveyTitle { get; set; } = string.Empty;

    public string SurveyDescription { get; set; } = string.Empty;

    public string AssessmentName { get; set; } = string.Empty;

    public List<SurveyThemeDto> Themes { get; set; } = new();

    public List<SurveyRoleDto> Roles { get; set; } = new();

    public List<SurveyRegionDto> Regions { get; set; } = new();

    public List<SurveyExperienceDto> Experiences { get; set; } = new();
}

public sealed class SurveyThemeDto
{
    public Guid ThemeId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<SurveyQuestionDto> Questions { get; set; } = new();
}

public sealed class SurveyQuestionDto
{
    public Guid QuestionId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string QuestionPolarity { get; set; } = string.Empty;

    public string QuestionType { get; set; } = string.Empty;

    public List<int> AnswerOptions { get; set; } = new();
}

public sealed class SurveyRoleDto
{
    public Guid RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;
}

public sealed class SurveyRegionDto
{
    public Guid RegionId { get; set; }

    public string RegionName { get; set; } = string.Empty;
}

public sealed class SurveyExperienceDto
{
    public Guid ExperienceId { get; set; }

    public string ExperienceValue { get; set; } = string.Empty;
}