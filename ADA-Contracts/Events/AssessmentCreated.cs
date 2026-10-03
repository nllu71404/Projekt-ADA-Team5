using ADA_Contracts.Other_contracts;
namespace ADA_Contracts.Events
{
    public record AssessmentCreated(Guid AssessmentId, List<RespondentEmailContract> Respondents);
    
}
