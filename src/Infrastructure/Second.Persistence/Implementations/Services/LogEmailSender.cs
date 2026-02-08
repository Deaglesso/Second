using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Second.Application.Contracts.Services;

namespace Second.Persistence.Implementations.Services
{
    public class LogEmailSender : IEmailSender
    {
        private readonly ILogger<LogEmailSender> _logger;

        public LogEmailSender(ILogger<LogEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Email dispatch to {ToEmail}. Subject: {Subject}. Body: {Body}", toEmail, subject, body);
            return Task.CompletedTask;
        }
    }
}
