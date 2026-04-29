using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.StudentNumber)
                .IsRequired()
                .HasMaxLength(20);

            
            builder.HasIndex(s => s.StudentNumber)
                .IsUnique();

            builder.Property(s => s.CurrentGPA)
                .HasPrecision(3, 2); 

            builder.HasOne(s => s.AppUser)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            
            builder.HasOne(s => s.AcademicProgram)
                .WithMany(p => p.Students)
                .HasForeignKey(s => s.AcademicProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(s => !s.IsDeleted);
        }
    }
}
