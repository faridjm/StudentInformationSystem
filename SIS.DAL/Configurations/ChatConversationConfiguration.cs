using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class ChatConversationConfiguration : IEntityTypeConfiguration<ChatConversation>
    {
        public void Configure(EntityTypeBuilder<ChatConversation> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(c => c.Group)
                .WithMany()
                .HasForeignKey(c => c.GroupId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(c => c.TeacherUser)
                .WithMany()
                .HasForeignKey(c => c.TeacherUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(c => c.StudentUser)
                .WithMany()
                .HasForeignKey(c => c.StudentUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(c => new { c.GroupId, c.TeacherUserId, c.StudentUserId }).IsUnique();
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
