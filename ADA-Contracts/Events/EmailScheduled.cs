using ADA_Contracts.Other_contracts;
using ADA_Contracts.Enums;

namespace ADA_Contracts.Events
{
    public record EmailScheduled(Guid AssessmentId, EmailType EmailType, string ApplicationName, DateTime StartDate, DateTime EndDate, DateTime SentDate, List<RespondentEmailContract> Respondents);
    
}
