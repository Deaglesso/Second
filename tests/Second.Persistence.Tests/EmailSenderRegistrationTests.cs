using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Second.Application.Contracts.Services;
using Second.Persistence;
using Second.Persistence.Implementations.Services;
using Xunit;

namespace Second.Persistence.Tests
{
    public sealed class EmailSenderRegistrationTests
    {
        [Fact]
        public void AddPersistence_UsesLogEmailSender_WhenEmailDisabled()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=SecondDb_Test;Trusted_Connection=True;",
                    ["Email:Enabled"] = "false"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddPersistence(configuration);

            using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
            using var scope = serviceProvider.CreateScope();

            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            Assert.IsType<LogEmailSender>(emailSender);
        }

        [Fact]
        public void AddPersistence_UsesSmtpEmailSender_WhenEmailEnabled()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=SecondDb_Test;Trusted_Connection=True;",
                    ["Email:Enabled"] = "true",
                    ["Email:FromAddress"] = "no-reply@example.com",
                    ["Email:FromName"] = "Second",
                    ["Email:SmtpHost"] = "smtp.example.com",
                    ["Email:SmtpPort"] = "587",
                    ["Email:UseSsl"] = "true",
                    ["Email:UseDefaultCredentials"] = "false",
                    ["Email:Username"] = "user",
                    ["Email:Password"] = "pass",
                    ["Email:TimeoutMilliseconds"] = "10000"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddPersistence(configuration);

            using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
            using var scope = serviceProvider.CreateScope();

            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            Assert.IsType<SmtpEmailSender>(emailSender);
        }
    }
}
