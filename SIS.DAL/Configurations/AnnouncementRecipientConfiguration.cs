using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class AnnouncementRecipientConfiguration : IEntityTypeConfiguration<AnnouncementRecipient>
    {
        public void Configure(EntityTypeBuilder<AnnouncementRecipient> builder)
        {
            builder.HasKey(r => r.Id);

            builder.HasOne(r => r.Announcement)
                .WithMany(a => a.Recipients)
                .HasForeignKey(r => r.AnnouncementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Eyni elan + eyni istifadəçi cütü unikal olmalıdır (dublikat recipient yox).
            builder.HasIndex(r => new { r.AnnouncementId, r.UserId }).IsUnique();
            builder.HasIndex(r => r.UserId);

            builder.HasQueryFilter(r => !r.IsDeleted);
        }
    }
}
