using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Second.Domain.Entities;

namespace Second.Persistence.Data.Configurations
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.HasKey(report => report.Id);

            builder.Property(report => report.Reason)
                .HasMaxLength(1000)
                .IsRequired();

            builder.HasIndex(report => new { report.TargetType, report.TargetId });
            builder.HasIndex(report => report.ReporterId);
        }
    }
}
