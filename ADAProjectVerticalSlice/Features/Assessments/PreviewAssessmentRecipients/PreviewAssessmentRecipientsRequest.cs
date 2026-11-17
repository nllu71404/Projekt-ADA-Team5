namespace ADAProjectAPIVerticalSlice.Features.Assessments.PreviewAssessmentRecipients;

public sealed record PreviewAssessmentRecipientsRequest(
    Guid[]? RegionIds,
    string?[]? RoleNames);