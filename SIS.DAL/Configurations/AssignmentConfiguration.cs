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
            builder.Property(a => a.MaxScore).HasColumnType("decimal(10,2)");

            builder.HasOne(a => a.Group)
                .WithMany(g => g.Assignments)
                .HasForeignKey(a => a.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.AssessmentItem)
                .WithMany()
                .HasForeignKey(a => a.AssessmentItemId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
