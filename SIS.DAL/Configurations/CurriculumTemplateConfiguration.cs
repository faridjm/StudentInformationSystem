using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class CurriculumTemplateConfiguration : IEntityTypeConfiguration<CurriculumTemplate>
    {
        public void Configure(EntityTypeBuilder<CurriculumTemplate> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Description).HasMaxLength(1000);

            builder.HasOne(t => t.Course)
                .WithMany()
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => t.CourseId);

            builder.HasQueryFilter(t => !t.IsDeleted);
        }
    }
}
