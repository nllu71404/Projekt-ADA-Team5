using System;
using System.Collections.Generic;
using System.Text;
using ADA_Contracts.Enums;

namespace ADA_EmailConsumer.Services;

public class TestEmailService : IEmailService
{
    private readonly ILogger<TestEmailService> _logger;

    public TestEmailService(ILogger<TestEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string recipient, string applicationName, DateTime startDate, DateTime endDate, DateTime sentDate, EmailType emailType, string subject, string surveyLink, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            """
            TEST EMAIL
            To: {Recipient}
            Application Name: {ApplicationName}
            Start Date: {StartDate}
            End Date: {EndDate}
            Sent Date: {SentDate}
            Email Type: {EmailType}
            Subject: {Subject}
            Survey link: {SurveyLink}
            """,
            recipient,
            applicationName,
            startDate,
            endDate,
            sentDate,
            emailType,
            subject,
            surveyLink);

        return Task.CompletedTask;
    }
}
