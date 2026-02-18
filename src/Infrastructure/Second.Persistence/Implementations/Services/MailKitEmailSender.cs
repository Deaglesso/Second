using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Second.Application.Contracts.Services;
using Second.Persistence.Configuration;

namespace Second.Persistence.Implementations.Services
{
    public sealed class MailKitEmailSender : IEmailSender
    {
        private readonly EmailOptions _options;
        private readonly ILogger<MailKitEmailSender> _logger;

        public MailKitEmailSender(IOptions<EmailOptions> options, ILogger<MailKitEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            var message = BuildMessage(toEmail, subject, body);

            using var smtpClient = new SmtpClient();
            smtpClient.Timeout = _options.TimeoutMilliseconds;

            var secureSocketOptions = ResolveSocketOptions(_options.SmtpPort, _options.UseSsl);

            try
            {
                await smtpClient.ConnectAsync(_options.SmtpHost, _options.SmtpPort, secureSocketOptions, cancellationToken);

                if (!_options.UseDefaultCredentials)
                {
                    await smtpClient.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
                }

                await smtpClient.SendAsync(message, cancellationToken);
                await smtpClient.DisconnectAsync(quit: true, cancellationToken);

                _logger.LogInformation("Email sent to {ToEmail} with subject {Subject} via {Host}:{Port}", toEmail, subject, _options.SmtpHost, _options.SmtpPort);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} with subject {Subject}", toEmail, subject);
                throw;
            }
        }

        private MimeMessage BuildMessage(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                throw new ArgumentException("Destination email is required.", nameof(toEmail));
            }

            if (!MailboxAddress.TryParse(toEmail, out var toMailbox))
            {
                throw new ArgumentException("Destination email address is invalid.", nameof(toEmail));
            }

            if (!MailboxAddress.TryParse(_options.FromAddress, out var fromMailbox))
            {
                throw new InvalidOperationException("Email sender address is invalid. Configure Email:FromAddress with a valid email address.");
            }

            fromMailbox.Name = _options.FromName;

            var message = new MimeMessage();
            message.From.Add(fromMailbox);
            message.To.Add(toMailbox);
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body ?? string.Empty };

            return message;
        }

        private static SecureSocketOptions ResolveSocketOptions(int smtpPort, bool useSsl)
        {
            if (!useSsl)
            {
                return SecureSocketOptions.None;
            }

            return smtpPort == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;
        }
    }
}
