using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Second.Application.Contracts.Repositories;
using Second.Application.Contracts.Services;
using Second.Domain.Entities;
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

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();

            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddSingleton<ITokenRevocationService>(_ =>
            {
                var redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379,abortConnect=false";

                try
                {
                    var options = ConfigurationOptions.Parse(redisConnectionString);
                    options.AbortOnConnectFail = false;
                    var multiplexer = ConnectionMultiplexer.Connect(options);
                    return new RedisTokenRevocationService(multiplexer);
                }
                catch
                {
                    return new NoOpTokenRevocationService();
                }
            });
            services.AddScoped<IEmailSender, LogEmailSender>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IEntityValidationService, EntityValidationService>();

            return services;
        }
    }
}
