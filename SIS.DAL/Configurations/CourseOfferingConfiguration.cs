using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class CourseOfferingConfiguration : IEntityTypeConfiguration<CourseOffering>
    {
        public void Configure(EntityTypeBuilder<CourseOffering> builder)
        {
            builder.HasKey(co => co.Id);

            builder.Property(co => co.Room)
                .HasMaxLength(50);

            
            builder.HasIndex(co => new { co.AcademicTermId, co.CourseId, co.TeacherId })
                .IsUnique();

            builder.HasOne(co => co.AcademicTerm)
                .WithMany(t => t.CourseOfferings)
                .HasForeignKey(co => co.AcademicTermId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(co => co.Course)
                .WithMany(c => c.CourseOfferings)
                .HasForeignKey(co => co.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(co => co.Teacher)
                .WithMany(t => t.CourseOfferings)
                .HasForeignKey(co => co.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(co => !co.IsDeleted);
        }
    }
}
