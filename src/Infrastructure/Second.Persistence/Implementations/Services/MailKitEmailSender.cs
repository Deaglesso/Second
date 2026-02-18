using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Second.Application.Contracts.Services;
using Second.Application.Exceptions;
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
                await AuthenticateIfSupportedAsync(smtpClient, cancellationToken);

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

        private async Task AuthenticateIfSupportedAsync(SmtpClient smtpClient, CancellationToken cancellationToken)
        {
            if (!smtpClient.Capabilities.HasFlag(SmtpCapabilities.Authentication))
            {
                _logger.LogDebug("SMTP server {Host}:{Port} does not advertise AUTH capability; skipping authentication.", _options.SmtpHost, _options.SmtpPort);
                return;
            }

            if (_options.UseDefaultCredentials)
            {
                await smtpClient.AuthenticateAsync(Encoding.UTF8, CredentialCache.DefaultNetworkCredentials, cancellationToken);
                return;
            }

            await smtpClient.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
        }

        private MimeMessage BuildMessage(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                throw new BadRequestAppException("Destination email is required.", "email_destination_required");
            }

            if (!MailboxAddress.TryParse(toEmail, out var toMailbox))
            {
                throw new BadRequestAppException("Destination email address is invalid.", "email_destination_invalid");
            }

            if (!MailboxAddress.TryParse(_options.FromAddress, out var fromMailbox))
            {
                throw new ConfigurationAppException("Email sender address is invalid. Configure Email:FromAddress with a valid email address.", "invalid_email_sender_address");
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
