using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class OnlineLessonConfiguration : IEntityTypeConfiguration<OnlineLesson>
    {
        public void Configure(EntityTypeBuilder<OnlineLesson> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Title).IsRequired().HasMaxLength(200);
            builder.Property(o => o.Description).HasMaxLength(1000);
            builder.Property(o => o.MeetingUrl).HasMaxLength(2048);

            // Group → OnlineLessons. Restrict: Group → ClassSession → OnlineLesson
            // çoxlu cascade yolunun qarşısını alır.
            builder.HasOne(o => o.Group)
                .WithMany(g => g.OnlineLessons)
                .HasForeignKey(o => o.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.ClassSession)
                .WithMany(s => s.OnlineLessons)
                .HasForeignKey(o => o.ClassSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Teacher)
                .WithMany()
                .HasForeignKey(o => o.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(o => o.GroupId);
            builder.HasIndex(o => o.ClassSessionId);
            builder.HasIndex(o => o.TeacherId);
            builder.HasIndex(o => o.Status);

            builder.HasQueryFilter(o => !o.IsDeleted);
        }
    }
}
