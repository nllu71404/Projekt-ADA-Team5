using System;
using System.Collections.Generic;
using System.Text;

namespace ADA_EmailConsumer.Services;

public class TestEmailService : IEmailService
{
    private readonly ILogger<TestEmailService> _logger;

    public TestEmailService(ILogger<TestEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string recipient, string applicationName, DateTime startDate, DateTime endDate, DateTime sentDate, string subject, string surveyLink, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            """
            TEST EMAIL
            To: {Recipient}
            Application Name: {ApplicationName}
            Start Date: {StartDate}
            End Date: {EndDate}
            Sent Date: {SentDate}
            Subject: {Subject}
            Survey link: {SurveyLink}
            """,
            recipient,
            applicationName,
            startDate,
            endDate,
            sentDate,
            subject,
            surveyLink);

        return Task.CompletedTask;
    }
}
