using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
    {
        public void Configure(EntityTypeBuilder<AcademicYear> builder)
        {
            builder.Property(y => y.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(y => y.Name).IsUnique();
            builder.HasQueryFilter(y => !y.IsDeleted);
        }
    }
}
