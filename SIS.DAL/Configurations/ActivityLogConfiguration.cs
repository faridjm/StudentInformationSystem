using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.EntityName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.OldValue)
                .HasColumnType("nvarchar(max)"); 

            builder.Property(a => a.NewValue)
                .HasColumnType("nvarchar(max)"); 

            builder.Property(a => a.IpAddress)
                .HasMaxLength(45); 

            
            builder.HasOne(a => a.User)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            
        }
    }

}
