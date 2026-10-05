using ADA_Contracts.Events;
using ADA_Contracts.Other_contracts;
using ADA_EmailConsumer.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADA_EmailConsumer.Consumers
{
    public class AssessmentCreatedConsumer
    {
        private readonly IEmailService _emailService;

        public AssessmentCreatedConsumer(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Handle(AssessmentCreated @event, CancellationToken cancellationToken)
        {
            var sentDate = DateTime.UtcNow;

            foreach (var respondent in @event.Respondents)
            {
                var surveyLink = CreateSurveyLink(
                    respondent.AccessToken);

                await _emailService.SendAsync(respondent.EmailAddress, @event.ApplicationName, @event.StartDate, @event.EndDate, sentDate, "Du er inviteret til en ADA-måling", surveyLink, cancellationToken);
            }
        }

        private static string CreateSurveyLink(string accessToken)
        {
            return $"https://localhost:7000/survey?token={accessToken}";
        }
    }
}
