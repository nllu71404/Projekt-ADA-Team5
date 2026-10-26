using System;
using System.Collections.Generic;
using System.Text;

namespace ADA_Contracts.Events;

    public record ResponseSubmitted(
        Guid AssessmentId,
        Guid RespondentId,
        string AssessmentName,
        string ApplicationName,
        string RoleName,
        string RegionName,
        string Experience,
        DateTime SubmittedAt,
        List<SubmittedAnswer> Answers,
        List<SubmittedComment> Comments
    );

public record SubmittedAnswer(
        Guid QuestionId,
        string QuestionText,
        string QuestionPolarity,
        int Point,
        Guid ThemeId,
        string ThemeTitle,
        string ThemeDescription
    );

public record SubmittedComment(
        Guid ThemeId,
        string ThemeTitle,
        string FreeForm
    );

