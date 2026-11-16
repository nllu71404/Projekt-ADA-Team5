using ADA_Contracts;
using ADA_Contracts.Enums;
using ADA_Contracts.Events;
using ADA_EmailConsumer.Services;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADA_EmailConsumer.Consumers;

public class EmailScheduledConsumer
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailScheduledConsumer> _logger;
    private readonly IDataProtector _tokenProtector;

    public EmailScheduledConsumer(
        IEmailService emailService,
        ILogger<EmailScheduledConsumer> logger,
        IDataProtectionProvider dataProtectionProvider)
    {
        _emailService = emailService;
        _logger = logger;
        _tokenProtector = dataProtectionProvider.CreateProtector(
       "ADA.MagicLinkToken.v1");
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
            // Pak det beskyttede token ud igen.
            var accessToken = _tokenProtector.Unprotect(
                respondent.ProtectedAccessToken);

            var surveyLink =
                $"https://localhost:7000/survey?token={Uri.EscapeDataString(accessToken)}";

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
                @event.ScheduledDate,
                @event.EmailType,
                subject,
                surveyLink,
                cancellationToken);
        }
    }
}
