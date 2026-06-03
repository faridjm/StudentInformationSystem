using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
    {
        public void Configure(EntityTypeBuilder<QuizAttempt> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Score).HasPrecision(5, 2);

            builder.HasOne(a => a.Quiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(a => a.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enrollment-də əks naviqasiya yoxdur; cascade yollarının toqquşmaması üçün Restrict.
            builder.HasOne(a => a.Enrollment)
                .WithMany()
                .HasForeignKey(a => a.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bir tələbə (enrollment) bir quiz-ə yalnız bir dəfə cəhd edə bilər.
            builder.HasIndex(a => new { a.QuizId, a.EnrollmentId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
