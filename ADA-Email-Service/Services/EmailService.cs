using System;
using System.Collections.Generic;
using System.Text;
using Resend;
using ADA_Contracts.Enums;
using ADA_Contracts;

namespace ADA_EmailConsumer.Services
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IResend resend,
            ILogger<EmailService> logger)
        {
            _resend = resend;
            _logger = logger;
        }

        public async Task SendAsync(
            string recipient,
            string applicationName,
            DateTime startDate,
            DateTime endDate,
            DateTime scheduledDate,
            EmailType emailType,
            string subject,
            string surveyLink,
            CancellationToken cancellationToken)
        {
            var body = emailType switch
            {
                EmailType.HeadsUp => $"""
                <h2>Du bliver snart inviteret til en ADA-måling</h2>

                <p>
                    Du vil snart modtage en invitation til at deltage
                    i en ADA-måling for:
                </p>

                <p><strong>{applicationName}</strong></p>

                <p>
                    Målingen åbner:
                    <strong>{startDate:dd/MM/yyyy HH:mm}</strong>
                </p>

                <p>
                    Målingen lukker:
                    <strong>{endDate:dd/MM/yyyy HH:mm}</strong>
                </p>

                <p>
                    Du modtager en ny email, når målingen åbner.
                </p>
                """,

                EmailType.Invitation => $"""
                <h2>Din ADA-måling er nu åben</h2>

                <p>
                    Du er inviteret til at deltage i en ADA-måling for:
                </p>

                <p><strong>{applicationName}</strong></p>

                <p>
                    Målingen er åben fra:
                    <strong>{startDate:dd/MM/yyyy HH:mm}</strong>
                </p>

                <p>
                    Målingen lukker:
                    <strong>{endDate:dd/MM/yyyy HH:mm}</strong>
                </p>

                <p>
                    <a href="{surveyLink}">
                        Gå til ADA-målingen
                    </a>
                </p>
                """,

                EmailType.Reminder => $"""
                <h2>Du mangler at besvare ADA-målingen</h2>

                <p>
                    Dette er en påmindelse om, at du endnu ikke har
                    besvaret ADA-målingen for:
                </p>

                <p><strong>{applicationName}</strong></p>

                <p>
                    Målingen lukker:
                    <strong>{endDate:dd/MM/yyyy HH:mm}</strong>
                </p>

                <p>
                    Du kan besvare målingen ved at klikke her:
                </p>

                <p>
                    <a href="{surveyLink}">
                        Gå til ADA-målingen
                    </a>
                </p>
                """,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(emailType),
                    emailType,
                    "Ukendt emailtype.")
            };

            var message = new EmailMessage();

            message.From = "ADA <onboarding@resend.dev>";
            message.To.Add(recipient);
            message.Subject = subject;
            message.HtmlBody = body;

            await _resend.EmailSendAsync(
                message,
                cancellationToken);

            _logger.LogInformation(
                "Email sent to {Recipient}. Email type: {EmailType}",
                recipient,
                emailType);
        }
    }
}
