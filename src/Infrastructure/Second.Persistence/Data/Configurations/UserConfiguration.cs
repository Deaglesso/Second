using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Second.Domain.Entities;

namespace Second.Persistence.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(user => user.Id);

            builder.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(320);

            builder.HasIndex(user => user.Email)
                .IsUnique();

            builder.Property(user => user.PasswordHash)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(user => user.Role)
                .IsRequired();

            builder.Property(user => user.SellerRating)
                .HasPrecision(2, 1)
                .HasDefaultValue(0.0m);

            builder.Property(user => user.ListingLimit)
                .IsRequired()
                .HasDefaultValue(10);

            builder.ToTable("Users", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_Users_SellerRating", "[SellerRating] >= 0 AND [SellerRating] <= 5");
                tableBuilder.HasCheckConstraint("CK_Users_ListingLimit", "[ListingLimit] >= 0");
            });

            builder.Property(user => user.EmailVerificationTokenHash)
                .HasMaxLength(256);

            builder.Property(user => user.PasswordResetTokenHash)
                .HasMaxLength(256);

            builder.Property(user => user.RefreshTokenHash)
                .HasMaxLength(256);
        }
    }
}
