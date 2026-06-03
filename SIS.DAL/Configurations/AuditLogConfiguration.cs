using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.UserId).HasMaxLength(450);
            builder.Property(a => a.UserFullName).HasMaxLength(200);
            builder.Property(a => a.UserRole).HasMaxLength(50);
            builder.Property(a => a.Action).IsRequired().HasMaxLength(50);
            builder.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
            builder.Property(a => a.EntityId).HasMaxLength(50);
            builder.Property(a => a.Description).HasMaxLength(1000);
            builder.Property(a => a.IpAddress).HasMaxLength(64);
            builder.Property(a => a.UserAgent).HasMaxLength(512);

            builder.HasIndex(a => a.CreatedAt);
            builder.HasIndex(a => a.Action);
            builder.HasIndex(a => a.EntityName);
            builder.HasIndex(a => a.UserId);

            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
