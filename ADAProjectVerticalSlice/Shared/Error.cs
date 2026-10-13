namespace ADAProjectAPIVerticalSlice.Shared
{
    public record Error(string Code, string Message)
    {
        public static readonly Error None = new("", "");

        public static readonly Error NullValue =
            new("Error.NullValue", "Den angivne værdi er null.");

        public static readonly Error ConditionNotMet =
            new("Error.ConditionNotMet", "Betingelsen er ikke opfyldt.");

        // SubmitSurveyResponse errors

        public static readonly Error InvalidRespondentToken =
            new("Survey.InvalidRespondentToken",
                "Respondentens token er ugyldigt.");

        public static readonly Error SurveyAlreadyAnswered =
            new("Survey.AlreadyAnswered",
                "Besvarelsen er allerede indsendt.");

        public static readonly Error SurveyNotOpen =
            new("Survey.NotOpen",
                "Målingen er ikke åben for besvarelser.");

        public static readonly Error NoAnswersProvided =
            new("Survey.NoAnswersProvided",
                "Der er ikke angivet nogen svar.");

        public static readonly Error InvalidQuestion =
            new("Survey.InvalidQuestion",
                "Et eller flere spørgsmål tilhører ikke denne survey.");

        public static readonly Error DuplicateAnswer =
            new("Survey.DuplicateAnswer",
                "Der må kun afgives ét svar pr. spørgsmål.");

        public static readonly Error InvalidAnswerPoint =
            new("Survey.InvalidAnswerPoint",
                "Alle svar skal have en værdi mellem 1 og 5.");

        public static readonly Error IncompleteAnswers =
            new("Survey.IncompleteAnswers",
                "Alle spørgsmål skal besvares.");

        public static readonly Error InvalidCommentTheme =
            new("Survey.InvalidCommentTheme",
                "En eller flere kommentarer tilhører et ugyldigt tema.");

        public static readonly Error DuplicateComment =
            new("Survey.DuplicateComment",
                "Der må kun angives én kommentar pr. tema.");
    }
}
