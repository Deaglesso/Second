using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Second.Domain.Entities;

namespace Second.Persistence.Data.Configurations
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.HasKey(image => image.Id);

            builder.Property(image => image.ImageUrl)
                .HasMaxLength(500)
                .IsRequired();

            builder.HasIndex(image => new { image.ProductId, image.Order })
                .IsUnique();
        }
    }
}
