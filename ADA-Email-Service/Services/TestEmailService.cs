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

    public Task SendAsync(
        string recipient,
        string subject,
        string surveyLink,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            """
            TEST EMAIL
            To: {Recipient}
            Subject: {Subject}
            Survey link: {SurveyLink}
            """,
            recipient,
            subject,
            surveyLink);

        return Task.CompletedTask;
    }
}
