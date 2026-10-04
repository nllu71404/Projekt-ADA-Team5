using System;
using System.Collections.Generic;
using System.Text;
using Resend;

namespace ADA_EmailConsumer.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IResend _resend;

        public EmailService(
            ILogger<EmailService> logger,
            IResend resend)
        {
            _logger = logger;
            _resend = resend;

            Console.WriteLine("ResendEmailService initialized.");
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

            var message = new EmailMessage
            {
                From = "onboarding@resend.dev",
                Subject = subject,
                HtmlBody = $"""
                    <h2>Du er inviteret til en ADA-måling</h2>

                    <p>Du er blevet inviteret til at deltage i en ADA-måling.</p>

                    <p>
                        <a href="{surveyLink}">
                            Klik her for at åbne målingen
                        </a>
                    </p>
                    """
            };

            message.To.Add(recipient);

            await _resend.EmailSendAsync(message);

            _logger.LogInformation(
                "Email sent successfully to {Recipient}",
                recipient);
        }
    }
}
