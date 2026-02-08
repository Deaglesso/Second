using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Second.Domain.Entities;

namespace Second.Persistence.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(product => product.Id);

            builder.Property(product => product.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(product => product.Description)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(product => product.PriceText)
                .HasMaxLength(100);

            builder.HasOne(product => product.SellerUser)
                .WithMany(user => user.Products)
                .HasForeignKey(product => product.SellerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(product => product.Images)
                .WithOne(image => image.Product)
                .HasForeignKey(image => image.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(product => product.ChatRooms)
                .WithOne(chatRoom => chatRoom.Product)
                .HasForeignKey(chatRoom => chatRoom.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
