using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Second.Application.Contracts.Services;
using Second.Persistence.Configuration;

namespace Second.Persistence.Implementations.Services
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(new MailAddress(toEmail));

            using var smtpClient = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                EnableSsl = _options.UseSsl,
                Timeout = _options.TimeoutMilliseconds,
                UseDefaultCredentials = _options.UseDefaultCredentials
            };

            if (!_options.UseDefaultCredentials)
            {
                smtpClient.Credentials = new NetworkCredential(_options.Username, _options.Password);
            }

            try
            {
                await smtpClient.SendMailAsync(message, cancellationToken);
                _logger.LogInformation("Email sent to {ToEmail} with subject {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} with subject {Subject}", toEmail, subject);
                throw;
            }
        }
    }
}
