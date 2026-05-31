using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class AssessmentSubmissionConfiguration : IEntityTypeConfiguration<AssessmentSubmission>
    {
        public void Configure(EntityTypeBuilder<AssessmentSubmission> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Score).HasColumnType("decimal(10,2)");

            builder.HasOne(a => a.AssessmentItem)
                .WithMany(i => i.Submissions)
                .HasForeignKey(a => a.AssessmentItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Enrollment)
                .WithMany()
                .HasForeignKey(a => a.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
