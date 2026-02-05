using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Second.Domain.Entities;

namespace Second.Persistence.Data.Configurations
{
    public class SellerProfileConfiguration : IEntityTypeConfiguration<SellerProfile>
    {
        public void Configure(EntityTypeBuilder<SellerProfile> builder)
        {
            builder.HasKey(profile => profile.Id);

            builder.Property(profile => profile.DisplayName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(profile => profile.Bio)
                .HasMaxLength(1000);

            builder.HasMany(profile => profile.Products)
                .WithOne(product => product.SellerProfile)
                .HasForeignKey(product => product.SellerProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
