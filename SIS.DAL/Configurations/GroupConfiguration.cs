using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.HasKey(g => g.Id);
            builder.Property(g => g.Name).IsRequired().HasMaxLength(50);
            builder.Property(g => g.DefaultMeetingUrl).HasMaxLength(2048);

            builder.HasOne(g => g.Course)
                .WithMany(c => c.Groups)
                .HasForeignKey(g => g.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(g => g.Teacher)
                .WithMany(t => t.Groups)
                .HasForeignKey(g => g.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(g => g.AcademicYear)
                .WithMany(a => a.Groups)
                .HasForeignKey(g => g.AcademicYearId)
                .OnDelete(DeleteBehavior.SetNull);

            // Real akademik otaq (scheduling). Otaq silinsə qrup RoomId = null olur.
            builder.HasOne(g => g.Room)
                .WithMany(r => r.Groups)
                .HasForeignKey(g => g.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(g => g.RoomId);

            builder.HasQueryFilter(g => !g.IsDeleted);
        }
    }
}
