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
            builder.Property(t => t.EmployeeNumber).IsRequired().HasMaxLength(20);
            builder.Property(t => t.Title).HasMaxLength(100);

            builder.HasOne(t => t.AppUser)
                .WithOne()
                .HasForeignKey<Teacher>(t => t.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(t => !t.IsDeleted);
        }
    }
}
