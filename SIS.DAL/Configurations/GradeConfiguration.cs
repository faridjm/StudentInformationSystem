using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class GradeConfiguration : IEntityTypeConfiguration<Grade>
    {
        public void Configure(EntityTypeBuilder<Grade> builder)
        {
            builder.HasKey(g => g.Id);

            builder.Property(g => g.Score)
                .HasPrecision(5, 2); 

            builder.Property(g => g.MaxScore)
                .HasPrecision(5, 2);

            builder.Property(g => g.Note)
                .HasMaxLength(300);

           
            builder.HasIndex(g => new { g.EnrollmentId, g.Type })
                .IsUnique();

            builder.HasOne(g => g.Enrollment)
                .WithMany(e => e.Grades)
                .HasForeignKey(g => g.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(g => !g.IsDeleted);
        }
    }
}
