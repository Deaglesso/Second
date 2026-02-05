using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Second.Application.Contracts.Repositories;
using Second.Application.Contracts.Services;
using Second.Persistence.Data;
using Second.Persistence.Implementations.Repositories;
using Second.Persistence.Implementations.Services;

namespace Second.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<ISellerProfileRepository, SellerProfileRepository>();
            services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ISellerProfileService, SellerProfileService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IReportService, ReportService>();

            return services;
        }
    }
}
