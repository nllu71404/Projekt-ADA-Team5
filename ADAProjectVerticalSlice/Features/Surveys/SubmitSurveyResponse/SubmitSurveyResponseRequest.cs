namespace ADAProjectAPIVerticalSlice.Features.Surveys.SubmitSurveyResponse;

public record SubmitSurveyResponseRequest(
    string Token,
    List<AnswerSubmission> Answers,
    List<CommentSubmission> Comments,
    Guid RoleId,
    Guid RegionId,
    Guid ExperienceId);

public record AnswerSubmission(
    Guid QuestionId,
    int Point);

public record CommentSubmission(
    Guid ThemeId,
    string FreeForm);
