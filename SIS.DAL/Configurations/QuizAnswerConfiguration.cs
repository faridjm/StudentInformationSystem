using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswer>
    {
        public void Configure(EntityTypeBuilder<QuizAnswer> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.SelectedOptionIds).HasMaxLength(500);
            builder.Property(a => a.PointsAwarded).HasPrecision(5, 2);

            builder.HasOne(a => a.QuizAttempt)
                .WithMany(at => at.Answers)
                .HasForeignKey(a => a.QuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            // QuizQuestion-da əks naviqasiya yoxdur; QuizAnswer-a yalnız bir cascade yolu olsun (Attempt vasitəsilə).
            builder.HasOne(a => a.QuizQuestion)
                .WithMany()
                .HasForeignKey(a => a.QuizQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
