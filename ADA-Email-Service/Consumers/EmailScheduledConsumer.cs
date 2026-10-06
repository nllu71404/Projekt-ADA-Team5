using ADA_Contracts;
using ADA_Contracts.Enums;
using ADA_Contracts.Events;
using ADA_EmailConsumer.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADA_EmailConsumer.Consumers;

public class EmailScheduledConsumer
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailScheduledConsumer> _logger;

    public EmailScheduledConsumer(
        IEmailService emailService,
        ILogger<EmailScheduledConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(
        EmailScheduled @event,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "EmailScheduled event received for Assessment {AssessmentId}. Email type: {EmailType}",
            @event.AssessmentId,
            @event.EmailType);

        foreach (var respondent in @event.Respondents)
        {
            var surveyLink =
                $"https://localhost:7000/survey?token={respondent.AccessToken}";

            var subject = @event.EmailType switch
            {
                EmailType.HeadsUp =>
                    "Du er inviteret til en ADA-måling",

                EmailType.Invitation =>
                    "Din ADA-måling er nu åben",

                EmailType.Reminder =>
                    "Påmindelse: Du mangler at besvare ADA-målingen",

                _ =>
                    "ADA-måling"
            };

            await _emailService.SendAsync(
                respondent.EmailAddress,
                @event.ApplicationName,
                @event.StartDate,
                @event.EndDate,
                @event.SentDate,
                @event.EmailType,
                subject,
                surveyLink,
                cancellationToken);
        }
    }
}
