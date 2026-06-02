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
            builder.Property(s => s.StudentNumber).IsRequired().HasMaxLength(20);
            builder.Property(s => s.CurrentGPA).HasPrecision(5, 2);

            builder.HasOne(s => s.AppUser)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(s => !s.IsDeleted);
        }
    }
}
