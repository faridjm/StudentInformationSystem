using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class CurriculumTemplateItemConfiguration : IEntityTypeConfiguration<CurriculumTemplateItem>
    {
        public void Configure(EntityTypeBuilder<CurriculumTemplateItem> builder)
        {
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Topic).IsRequired().HasMaxLength(300);
            builder.Property(i => i.AssessmentTitle).HasMaxLength(200);
            builder.Property(i => i.WeightPercent).HasPrecision(5, 2);
            builder.Property(i => i.MaxScore).HasPrecision(10, 2);

            builder.HasOne(i => i.CurriculumTemplate)
                .WithMany(t => t.Items)
                .HasForeignKey(i => i.CurriculumTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(i => new { i.CurriculumTemplateId, i.SessionNumber });

            builder.HasQueryFilter(i => !i.IsDeleted);
        }
    }
}
