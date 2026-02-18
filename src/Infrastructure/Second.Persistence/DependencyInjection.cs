using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Second.Application.Contracts.Repositories;
using Second.Application.Contracts.Services;
using Second.Domain.Entities;
using Second.Persistence.Configuration;
using Second.Persistence.Data;
using Second.Persistence.Implementations.Repositories;
using Second.Persistence.Implementations.Services;
using StackExchange.Redis;

namespace Second.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services
                .AddOptions<EmailOptions>()
                .Bind(configuration.GetSection(EmailOptions.SectionName))
                .Validate(options =>
                    !options.Enabled ||
                    (IsValidEmailAddress(options.FromAddress) &&
                     !string.IsNullOrWhiteSpace(options.SmtpHost) &&
                     options.SmtpPort is > 0 and <= 65535 &&
                     options.TimeoutMilliseconds is >= 1000 and <= 120000 &&
                     (options.UseDefaultCredentials ||
                      (!string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password)))),
                    "Invalid Email configuration. Ensure required SMTP settings are provided when Email:Enabled is true.")
                .ValidateOnStart();

            var redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
                redisOptions.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(redisOptions);
            });

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();

            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ITokenRevocationService, RedisTokenRevocationService>();
            services.AddScoped<IEmailSender>(serviceProvider =>
            {
                var emailOptions = serviceProvider.GetRequiredService<IOptions<EmailOptions>>().Value;
                return emailOptions.Enabled
                    ? serviceProvider.GetRequiredService<MailKitEmailSender>()
                    : serviceProvider.GetRequiredService<LogEmailSender>();
            });

            services.AddScoped<LogEmailSender>();
            services.AddScoped<MailKitEmailSender>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IEntityValidationService, EntityValidationService>();

            return services;
        }

        private static bool IsValidEmailAddress(string email)
        {
            try
            {
                _ = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
