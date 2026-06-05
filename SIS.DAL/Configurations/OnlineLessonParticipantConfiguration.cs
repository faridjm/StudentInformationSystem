using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class OnlineLessonParticipantConfiguration : IEntityTypeConfiguration<OnlineLessonParticipant>
    {
        public void Configure(EntityTypeBuilder<OnlineLessonParticipant> builder)
        {
            builder.HasKey(p => p.Id);

            builder.HasOne(p => p.OnlineLesson)
                .WithMany(o => o.Participants)
                .HasForeignKey(p => p.OnlineLessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bir tələbə bir online dərsdə yalnız bir dəfə iştirakçı ola bilər.
            builder.HasIndex(p => new { p.OnlineLessonId, p.StudentId }).IsUnique();

            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
