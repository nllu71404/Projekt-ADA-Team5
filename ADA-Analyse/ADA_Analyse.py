# Assessment and survey analytics API.
# Reads assessment/respondent data from PostgreSQL, calculates metrics with
# pandas, and exposes the results as JSON through FastAPI.
# Database configuration comes from ADA_ANALYSIS_DATABASE_URL.

# Access environment variables, including the database connection URL.
import os
# pandas provides DataFrames, date calculations, grouping, and summary statistics.
import pandas as pd
# psycopg is the PostgreSQL database driver.
import psycopg

# Read environment variables from a local .env file when one exists.
from dotenv import load_dotenv
# FastAPI defines the HTTP application and its routes.
from fastapi import FastAPI
# Validate assessment IDs supplied in URLs as UUIDs.
from uuid import UUID
# Return meaningful HTTP errors (for example, 404) to API clients.
from fastapi import HTTPException

# Load .env variables before creating database connections.
load_dotenv()

# Create the application instance used by an ASGI server such as Uvicorn.
app = FastAPI()


# Fetch the high-level data needed by the /analysis endpoint.
# Returns two DataFrames: assessments and respondents.
def get_database_data():

    # Open a database connection; the context manager closes it on exit.
    # os.environ[...] raises KeyError if the URL is not configured.
    with psycopg.connect(
        os.environ["ADA_ANALYSIS_DATABASE_URL"]
    ) as connection:

        # Use a cursor to execute SQL and retrieve rows.
        with connection.cursor() as cursor:

            # Retrieve assessments
            # Select the ID, display name, and date range of every assessment.
            cursor.execute("""
                SELECT
                    "AssessmentId",
                    "AssessmentName",
                    "StartDate",
                    "EndDate"
                FROM "Assessment"
            """)

            # fetchall() returns all matching records from the previous query.
            assessment_rows = cursor.fetchall()

            # Retrieve respondent status
            # Only fetch the columns needed to calculate response counts/rates.
            cursor.execute("""
                SELECT
                    "AssessmentId",
                    "HasAnswered"
                FROM "Respondent"
            """)

            # Keep these rows for conversion into a pandas DataFrame.
            respondent_rows = cursor.fetchall()

    # Turn raw assessment tuples into a labeled table.
    assessments = pd.DataFrame(
        assessment_rows,
        columns=[
            "AssessmentId",
            "AssessmentName",
            "StartDate",
            "EndDate"
        ]
    )

    # Turn raw respondent tuples into a table using the same assessment IDs.
    respondents = pd.DataFrame(
        respondent_rows,
        columns=[
            "AssessmentId",
            "HasAnswered"
        ]
    )

    # Return both tables together (a tuple) to the caller.
    return assessments, respondents



# Return part as a percentage of total, rounded to two decimal places.
def calculate_percentage(part, total):
    # Avoid dividing by zero when there are no relevant records.
    if total == 0:
        return 0.0

    # Multiplying by 100 changes a ratio (0-1) into a percentage (0-100).
    return round(part / total * 100, 2)


# Calculate high-level assessment and respondent metrics for /analysis.
def calculate_statistics():

    # Get a fresh snapshot of the two database tables for this request.
    assessments, respondents = get_database_data()

    # Use a timezone-aware UTC timestamp for consistent date comparisons.
    now = pd.Timestamp.now(tz="UTC")

    # ==================================
    # Assessment statistics
    # ==================================

    # Total number of assessment rows, regardless of their dates.
    total_assessments = len(assessments)

    # Initialize defaults so an empty dataset still returns valid results.
    active = 0
    upcoming = 0
    past = 0
    average_duration = 0.0
    monthly_data = []

    # Only run date operations when there is at least one assessment.
    if not assessments.empty:

        # Parse start dates and normalize them to UTC.
        assessments["StartDate"] = pd.to_datetime(
            assessments["StartDate"], utc=True
        )

        # Parse end dates the same way, avoiding timezone mismatch errors.
        assessments["EndDate"] = pd.to_datetime(
            assessments["EndDate"], utc=True
        )

        # An assessment is active when start <= now <= end.
        # & applies an element-by-element AND; sum() counts True values.
        active = int((
            (assessments["StartDate"] <= now) &
            (assessments["EndDate"] >= now)
        ).sum())

        # Upcoming assessments have not reached their start date.
        upcoming = int(
            (assessments["StartDate"] > now).sum()
        )

        # Past assessments have already passed their end date.
        past = int(
            (assessments["EndDate"] < now).sum()
        )

        # Calculate each assessment duration in days.
        # There are 86,400 seconds in one day.
        durations = (
            assessments["EndDate"] -
            assessments["StartDate"]
        ).dt.total_seconds() / 86400

        # Exclude negative durations caused by end dates before start dates.
        valid_durations = durations[durations >= 0]

        # Take the mean of valid durations; otherwise keep the 0.0 default.
        if not valid_durations.empty:
            average_duration = round(
                float(valid_durations.mean()), 2
            )

        # Convert each start date to a year-month label, e.g. "2026-11".
        months = assessments[
            "StartDate"
        ].dt.strftime("%Y-%m")

        # Count assessments by start month and sort chronologically.
        monthly_counts = (
            assessments.groupby(months)
            .size()
            .sort_index()
        )

        # Convert the grouped counts to JSON-friendly month/count objects.
        monthly_data = [
            {
                "month": month,
                "count": int(count)
            }
            for month, count in monthly_counts.items()
        ]

    # ==================================
    # Respondent statistics
    # ==================================

    # Total number of respondent rows across all assessments.
    total_respondents = len(respondents)

    # Defaults are returned if the Respondent table is empty.
    answered = 0
    unanswered = 0
    response_rate = 0.0
    responses_by_assessment = []

    # Process response status only when respondent records exist.
    if not respondents.empty:

        # Treat a missing HasAnswered value as False, then convert to Boolean.
        respondents["HasAnswered"] = (
            respondents["HasAnswered"]
            .fillna(False)
            .astype(bool)
        )

        # Boolean True behaves like 1, so sum() counts answered respondents.
        answered = int(
            respondents["HasAnswered"].sum()
        )

        # Everyone who has not answered is counted as unanswered.
        unanswered = total_respondents - answered

        # Calculate the global percentage of answered respondents.
        response_rate = calculate_percentage(
            answered, total_respondents
        )

        # For each AssessmentId, count all respondents and those answered.
        # size = row count; sum = number of True HasAnswered values.
        # to_dict("index") makes an ID -> {total, answered} lookup.
        grouped = (
            respondents.groupby("AssessmentId")
            .agg(
                total=("HasAnswered", "size"),
                answered=("HasAnswered", "sum")
            )
            .to_dict("index")
        )

    # No respondents means no counts to look up per assessment.
    else:
        grouped = {}

    # Response rate for every assessment,
    # including assessments with zero respondents
    # Iterate over assessment rows without their pandas index.
    # The underscores ignore StartDate and EndDate, which are unused here.
    for assessment_id, name, _, _ in (
        assessments.itertuples(index=False, name=None)
    ):

        # Default to an empty count record if this assessment has no respondents.
        counts = grouped.get(assessment_id, {})

        # Pull this assessment's total and completed response counts.
        total = int(counts.get("total", 0))
        completed = int(counts.get("answered", 0))

        # Produce one output entry per assessment, including zero-response ones.
        # UUID objects are converted to strings so JSON can represent them.
        responses_by_assessment.append({
            "assessmentId": str(assessment_id),
            "assessmentName": name,
            "totalRespondents": total,
            "answeredRespondents": completed,
            "responseRatePercent": calculate_percentage(
                completed, total
            )
        })

    # All statistics are calculated in Python
    # Return a dictionary; FastAPI will serialize it to a JSON response.
    # Keys before the blank line describe assessments; the rest describe respondents.
    return {
        "totalAssessments": total_assessments,
        "activeAssessments": active,
        "upcomingAssessments": upcoming,
        "pastAssessments": past,
        "averageDurationDays": average_duration,
        "assessmentsByMonth": monthly_data,

        "totalRespondents": total_respondents,
        "answeredRespondents": answered,
        "unansweredRespondents": unanswered,
        "responseRatePercent": response_rate,
        "responsesByAssessment": responses_by_assessment
    }

# The following functions support the assessment-specific survey endpoint.
# They are separate from the general /analysis calculations.
    # ==================================
    # Survey statistics
    # ==================================

# Read all qualifying scale-question answers for one assessment.
# assessment_id is already a UUID when provided by the FastAPI route.
def get_survey_answer_data(assessment_id: UUID):
    # Open a separate database connection for this detailed query.
    with psycopg.connect(
        os.environ["ADA_ANALYSIS_DATABASE_URL"]
    ) as connection:
        with connection.cursor() as cursor:

            # First confirm that the requested assessment actually exists.
            # %s is a parameter placeholder, not string interpolation.
            cursor.execute(
                'SELECT 1 FROM "Assessment" '
                'WHERE "AssessmentId" = %s',
                (assessment_id,)
            )

            # If the lookup returned no row, respond with HTTP 404.
            if cursor.fetchone() is None:
                raise HTTPException(
                    status_code=404,
                    detail="Assessment not found"
                )

            # Fetch scale answers along with their question/theme details and
            # optional respondent demographics (role, region, experience).
            # INNER JOINs require matching question, theme, respondent and
            # assessment/survey records. LEFT JOINs keep respondents even
            # when a demographic attribute is missing.
            # Filters restrict results to this assessment, completed
            # respondents, and questions whose type is "Scale".
            cursor.execute("""
                SELECT
                    a."QuestionId",
                    q."QuestionText",
                    q."QuestionPolarity",
                    q."ThemeId",
                    t."Title" AS "ThemeTitle",
                    a."RespondentId",
                    a."Point",
                    rol."RoleName",
                    reg."RegionName",
                    exp."ExperienceValue"
                FROM "Answer" a
                JOIN "Question" q
                    ON q."QuestionId" = a."QuestionId"
                JOIN "Theme" t
                    ON t."ThemeId" = q."ThemeId"
                JOIN "Respondent" r
                    ON r."RespondentId" = a."RespondentId"
                JOIN "Assessment" ass
                    ON ass."AssessmentId" = r."AssessmentId"
                    AND ass."SurveyId" = q."SurveyId"
                LEFT JOIN "Roles" rol
                    ON rol."RoleId" = r."RoleId"
                LEFT JOIN "Regions" reg
                    ON reg."RegionId" = r."RegionId"
                LEFT JOIN "Experiences" exp
                    ON exp."ExperienceId" = r."ExperienceId"
                WHERE r."AssessmentId" = %s
                    AND r."HasAnswered" = TRUE
                    AND q."QuestionType" = 'Scale'
            """, (assessment_id,))

            # Fetch all answer rows, then read the returned SQL column names.
            rows = cursor.fetchall()
            columns = [
                column.name for column in cursor.description
            ]

    # Return the results as a DataFrame ready for statistical processing.
    return pd.DataFrame(rows, columns=columns)


# Summarize normalized 1-to-5 scores for any subset of answers.
# frame must contain AdjustedPoint and RespondentId columns.
def score_summary(frame):
    # Use integers so scores can be counted and compared consistently.
    scores = frame["AdjustedPoint"].astype(int)

    # Count how many times each score 1, 2, 3, 4, or 5 appears.
    # reindex fills missing score categories with zero.
    counts = scores.value_counts().reindex(
        range(1, 6), fill_value=0
    )

    # Generate a JSON-friendly collection of summary measures.
    return {
        # Number of answer rows (one respondent may contribute multiple answers).
        "answerCount": int(len(scores)),
        # Number of unique people represented by these answers.
        "respondentCount": int(
            frame["RespondentId"].nunique()
        ),
        # Arithmetic mean of all answers in this group.
        "averageScore": round(float(scores.mean()), 2),
        # Median (middle score when scores are ordered).
        "medianScore": round(float(scores.median()), 2),
        # Sample standard deviation, using n - 1 (ddof=1).
        # It is undefined for a single answer, so return None in that case.
        "standardDeviation": (
            round(float(scores.std(ddof=1)), 2)
            if len(scores) > 1 else None
        ),
        # Share of scores that are 4 or 5 (treated as favorable).
        "favorablePercent": round(
            float(scores.ge(4).mean() * 100), 2
        ),
        # Create a full distribution for charting or displaying scores.
        "distribution": [
            {"score": int(score), "count": int(count)}
            for score, count in counts.items()
        ]
    }


# Build detailed survey results for a specific assessment ID.
def calculate_survey_analysis(assessment_id: UUID):
    # Load the answers that passed the SQL filters above.
    df = get_survey_answer_data(assessment_id)

    # Count distinct respondents who have at least one qualifying scale answer.
    # This is not necessarily all respondents marked HasAnswered in the DB.
    completed = int(df["RespondentId"].nunique())

    # Minimum sample size for privacy
    # Do not publish statistics for fewer than five qualifying respondents.
    # Keep the same response fields even when the threshold is not met.
    if completed < 5:
        return {
            "assessmentId": str(assessment_id),
            "completedRespondents": completed,
            "message": "At least 5 completed surveys are required.",
            "overall": None,
            "themes": [],
            "questions": [],
            "byRole": [],
            "byRegion": [],
            "byExperience": []
        }

    # For reverse-worded questions, convert scores so a higher number
    # consistently points in the same direction as other questions.
    # Normalize reverse-scored questions
    # Ensure raw point values can be used in arithmetic.
    df["Point"] = df["Point"].astype(int)

    # Match the word "reverse" without regard to letter case.
    is_reverse = (
        df["QuestionPolarity"]
        .str.casefold()
        .eq("reverse")
    )

    # Normal questions retain their original score.
    # Reverse questions use 6 - score: 1<->5, 2<->4, and 3 stays 3.
    df["AdjustedPoint"] = df["Point"].where(
        ~is_reverse, 6 - df["Point"]
    )

    # Statistics by theme
    # Group all answers by theme ID and title, and summarize each theme.
    # Note: theme-level groups are not independently checked for >=5
    # respondents; only the overall assessment threshold was checked.
    themes = [
        {
            "themeId": str(theme_id),
            "title": title,
            "statistics": score_summary(group)
        }
        for (theme_id, title), group in df.groupby(
            ["ThemeId", "ThemeTitle"]
        )
    ]

    # Statistics by question
    # Group by question ID and text for per-question statistics.
    # Like themes, individual questions have no separate sample-size check.
    questions = [
        {
            "questionId": str(question_id),
            "questionText": text,
            "statistics": score_summary(group)
        }
        for (question_id, text), group in df.groupby(
            ["QuestionId", "QuestionText"]
        )
    ]

    # Group comparisons
    # This helper runs the same comparison for a chosen demographic column.
    # pandas groupby normally omits rows with missing group values.
    def compare(column):
        return [
            {
                "name": str(name),
                "statistics": score_summary(group)
            }
            # Only show a demographic subgroup if >=5 different respondents
            # contributed answers, limiting exposure of small groups.
            for name, group in df.groupby(column)
            if group["RespondentId"].nunique() >= 5
        ]

    # Assemble the complete endpoint response.
    # "overall" summarizes all scale answers, not respondent averages.
    return {
        "assessmentId": str(assessment_id),
        "completedRespondents": completed,
        "message": None,
        "overall": score_summary(df),
        "themes": themes,
        "questions": questions,
        "byRole": compare("RoleName"),
        "byRegion": compare("RegionName"),
        "byExperience": compare("ExperienceValue")
    }


# Handle GET /analysis/assessment/<UUID> with detailed survey statistics.
# FastAPI validates the path parameter and converts it to a UUID.
@app.get("/analysis/assessment/{assessment_id}")
def get_assessment_analysis(assessment_id: UUID):
    # Delegate the data retrieval and calculations to the helper above.
    return calculate_survey_analysis(assessment_id)



# Handle GET /analysis with overall assessment/respondent statistics.
@app.get("/analysis")
def get_analysis():
    # The returned dictionary is automatically converted to JSON.
    return calculate_statistics()