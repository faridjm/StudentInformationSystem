using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class WeeklySchedulePatternConfiguration : IEntityTypeConfiguration<WeeklySchedulePattern>
    {
        public void Configure(EntityTypeBuilder<WeeklySchedulePattern> builder)
        {
            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.Property(p => p.Room).HasMaxLength(50);

            builder.HasOne(p => p.CourseOffering)
                .WithMany(co => co.WeeklySchedules)
                .HasForeignKey(p => p.CourseOfferingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bir kursda eyni günün eyni saatla təkrarı olmamalıdır
            builder.HasIndex(p => new { p.CourseOfferingId, p.DayOfWeek, p.StartTime })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        }
    }
}
