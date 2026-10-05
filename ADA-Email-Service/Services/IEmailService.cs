using System;
using System.Collections.Generic;
using System.Text;

namespace ADA_EmailConsumer.Services
{
    public interface IEmailService
    {
        Task SendAsync(string recipient, string applicationName, DateTime startDate, DateTime endDate, DateTime sentDate, string subject, string surveyLink, CancellationToken cancellationToken);
    }
}
