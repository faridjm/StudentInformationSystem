using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
    {
        public void Configure(EntityTypeBuilder<ClassSession> builder)
        {
            builder.HasQueryFilter(s => !s.IsDeleted);

            builder.Property(s => s.Room).HasMaxLength(50);
            builder.Property(s => s.Topic).HasMaxLength(200);
            builder.Property(s => s.Notes).HasMaxLength(2000);
            builder.Property(s => s.CancellationReason).HasMaxLength(500);

            builder.HasOne(s => s.CourseOffering)
                .WithMany(co => co.ClassSessions)
                .HasForeignKey(s => s.CourseOfferingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Makeup sessiya öz orijinalını göstərə bilər
            builder.HasOne(s => s.OriginalSession)
                .WithMany()
                .HasForeignKey(s => s.OriginalSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bir kursda eyni gündə eyni başlama saatı təkrarı olmamalıdır
            builder.HasIndex(s => new { s.CourseOfferingId, s.Date, s.StartTime })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Sürətli axtarış üçün indeks
            builder.HasIndex(s => s.Date);
        }
    }
}
