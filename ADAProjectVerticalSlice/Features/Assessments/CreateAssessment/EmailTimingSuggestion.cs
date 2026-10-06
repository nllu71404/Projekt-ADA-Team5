namespace ADAProjectAPIVerticalSlice.Features.Assessments.CreateAssessment
{
    public record EmailTimingSuggestion(
    DateTime HeadsUpDate,
    DateTime InvitationDate,
    DateTime ReminderDate,
    List<string> Warnings);
}
