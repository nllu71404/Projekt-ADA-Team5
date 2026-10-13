namespace ADAProjectAPIVerticalSlice.Features.Surveys.SubmitSurveyResponse;

public record SubmitSurveyResponseRequest(
    string Token,
    List<AnswerSubmission> Answers,
    List<CommentSubmission> Comments);

public record AnswerSubmission(
    Guid QuestionId,
    int Point);

public record CommentSubmission(
    Guid ThemeId,
    string FreeForm);
