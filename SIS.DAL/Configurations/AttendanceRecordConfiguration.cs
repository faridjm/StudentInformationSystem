using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
    {
        public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Note)
                .HasMaxLength(300);

           
            builder.HasIndex(a => new { a.EnrollmentId, a.SessionDate })
                .IsUnique();

            builder.HasOne(a => a.Enrollment)
                .WithMany(e => e.AttendanceRecords)
                .HasForeignKey(a => a.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ClassSession)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(a => a.ClassSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => a.ClassSessionId);

            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
