using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
            builder.Property(d => d.Description).HasMaxLength(1000);
            builder.HasQueryFilter(d => !d.IsDeleted);

            // Department → Courses (one-to-many). DepartmentId optional (nullable):
            // kurs silinmədən şöbə dəyişdirilə/boşaldıla bilər.
            builder.HasMany(d => d.Courses)
                   .WithOne(c => c.Department!)
                   .HasForeignKey(c => c.DepartmentId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
