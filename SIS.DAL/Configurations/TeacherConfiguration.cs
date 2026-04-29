using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
    {
        public void Configure(EntityTypeBuilder<Teacher> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.EmployeeNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(t => t.EmployeeNumber)
                .IsUnique();

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(t => t.AppUser)
                .WithOne(u => u.Teacher)
                .HasForeignKey<Teacher>(t => t.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.Department)
                .WithMany(d => d.Teachers)
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(t => !t.IsDeleted);
        }
    }
}
