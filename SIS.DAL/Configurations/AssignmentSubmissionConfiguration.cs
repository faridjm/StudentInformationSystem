using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class AssignmentSubmissionConfiguration : IEntityTypeConfiguration<AssignmentSubmission>
    {
        public void Configure(EntityTypeBuilder<AssignmentSubmission> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Comment).HasMaxLength(2000);
            builder.Property(s => s.FileUrl).HasMaxLength(500);
            builder.Property(s => s.Feedback).HasMaxLength(1000);
            builder.Property(s => s.Score).HasPrecision(5, 2);

            builder.HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Enrollment)
                .WithMany(e => e.AssignmentSubmissions)
                .HasForeignKey(s => s.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bir tələbə bir tapşırığa yalnız bir dəfə cavab verə bilər
            builder.HasIndex(s => new { s.AssignmentId, s.EnrollmentId }).IsUnique();

            builder.HasQueryFilter(s => !s.IsDeleted);
        }
    }
}
