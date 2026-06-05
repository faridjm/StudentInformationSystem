using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class OnlineLessonChatMessageConfiguration : IEntityTypeConfiguration<OnlineLessonChatMessage>
    {
        public void Configure(EntityTypeBuilder<OnlineLessonChatMessage> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.MessageText).IsRequired().HasMaxLength(2000);
            builder.Property(m => m.SenderUserId).IsRequired();

            builder.HasOne(m => m.OnlineLesson)
                .WithMany(o => o.ChatMessages)
                .HasForeignKey(m => m.OnlineLessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(m => new { m.OnlineLessonId, m.SentAt });

            builder.HasQueryFilter(m => !m.IsDeleted);
        }
    }
}
