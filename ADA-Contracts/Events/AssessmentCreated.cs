
namespace ADA_Contracts.Events
{
    public record AssessmentCreated(
        Guid AssessmentId,
        string AssessmentName,
        DateTime StartDate,
        DateTime EndDate
       )
    {

    }
}
