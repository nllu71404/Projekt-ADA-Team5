using System;
using System.Collections.Generic;
using System.Text;

namespace ADA_EmailConsumer.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendAsync(
            string recipient,
            string subject,
            string surveyLink,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Sending email to {Recipient}",
                recipient);

            _logger.LogInformation(
                "Subject: {Subject}",
                subject);

            _logger.LogInformation(
                "Survey link: {SurveyLink}",
                surveyLink);

            await Task.CompletedTask;
        }
    }
}
