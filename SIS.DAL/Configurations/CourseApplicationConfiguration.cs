using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class CourseApplicationConfiguration : IEntityTypeConfiguration<CourseApplication>
    {
        public void Configure(EntityTypeBuilder<CourseApplication> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.FullName).IsRequired().HasMaxLength(150);
            builder.Property(a => a.Email).IsRequired().HasMaxLength(200);
            builder.Property(a => a.Phone).IsRequired().HasMaxLength(40);
            builder.Property(a => a.Message).HasMaxLength(2000);
            builder.Property(a => a.AdminNote).HasMaxLength(2000);
            builder.Property(a => a.Status).HasConversion<int>();

            builder.HasOne(a => a.Course)
                .WithMany()
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(a => a.Group)
                .WithMany()
                .HasForeignKey(a => a.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(a => a.ConvertedStudent)
                .WithMany()
                .HasForeignKey(a => a.ConvertedStudentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(a => a.Status);
            builder.HasIndex(a => a.CreatedAt);
            builder.HasIndex(a => a.Email);

            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
