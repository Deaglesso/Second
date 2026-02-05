using Microsoft.EntityFrameworkCore;
using Second.Domain.Entities;

namespace Second.Application.Contracts.Persistence
{
    public interface IAppDbContext
    {
        DbSet<Product> Products { get; }

        DbSet<ProductImage> ProductImages { get; }

        DbSet<SellerProfile> SellerProfiles { get; }

        DbSet<ChatRoom> ChatRooms { get; }

        DbSet<Message> Messages { get; }

        DbSet<Report> Reports { get; }
    }
}
