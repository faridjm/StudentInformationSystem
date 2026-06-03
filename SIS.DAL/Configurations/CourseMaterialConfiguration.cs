using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class CourseMaterialConfiguration : IEntityTypeConfiguration<CourseMaterial>
    {
        public void Configure(EntityTypeBuilder<CourseMaterial> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Description).HasMaxLength(1000);
            builder.Property(m => m.FilePath).HasMaxLength(500);
            builder.Property(m => m.FileName).HasMaxLength(300);
            builder.Property(m => m.LinkUrl).HasMaxLength(1000);

            // Group-a FK (Group-da əks naviqasiya yoxdur; cascade yollarının sadəliyi üçün Restrict).
            builder.HasOne(m => m.Group)
                .WithMany()
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(m => m.GroupId);

            builder.HasQueryFilter(m => !m.IsDeleted);
        }
    }
}
