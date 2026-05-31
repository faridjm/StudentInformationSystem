using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class AssessmentItemConfiguration : IEntityTypeConfiguration<AssessmentItem>
    {
        public void Configure(EntityTypeBuilder<AssessmentItem> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
            builder.Property(a => a.MaxScore).HasColumnType("decimal(10,2)");
            builder.Property(a => a.WeightPercent).HasColumnType("decimal(5,2)");

            builder.HasOne(a => a.Group)
                .WithMany(g => g.AssessmentItems)
                .HasForeignKey(a => a.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
