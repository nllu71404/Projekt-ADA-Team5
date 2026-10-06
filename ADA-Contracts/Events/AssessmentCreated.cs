using System;
using System.Collections.Generic;
using System.Text;
using ADA_Contracts.Other_contracts;

namespace ADA_Contracts.Events
{
    public record AssessmentCreated(Guid AssessmentId, string ApplicationName, DateTime StartDate, DateTime EndDate, List<RespondentEmailContract> Respondents);
}
