using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
    {
        public void Configure(EntityTypeBuilder<Assignment> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
            builder.Property(a => a.Description).HasMaxLength(2000);
            builder.Property(a => a.MaxScore).HasPrecision(5, 2);

            builder.HasOne(a => a.CourseOffering)
                .WithMany(co => co.Assignments)
                .HasForeignKey(a => a.CourseOfferingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
