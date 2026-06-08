using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class TranscriptVerificationConfiguration : IEntityTypeConfiguration<TranscriptVerification>
    {
        public void Configure(EntityTypeBuilder<TranscriptVerification> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.VerificationCode).IsRequired().HasMaxLength(64);
            builder.Property(v => v.Language).IsRequired().HasMaxLength(8);
            builder.Property(v => v.TranscriptType).HasConversion<int>();
            builder.Property(v => v.CompletionLevel).IsRequired().HasMaxLength(40);
            builder.Property(v => v.CertificateStatus).IsRequired().HasMaxLength(40);

            builder.Property(v => v.OfficialGrade).HasColumnType("decimal(5,2)");
            builder.Property(v => v.GPA).HasColumnType("decimal(4,2)");

            builder.HasIndex(v => v.VerificationCode).IsUnique();
            builder.HasIndex(v => v.StudentId);
            builder.HasIndex(v => v.GroupId);
            builder.HasIndex(v => v.EnrollmentId);

            builder.HasQueryFilter(v => !v.IsDeleted);
        }
    }
}
