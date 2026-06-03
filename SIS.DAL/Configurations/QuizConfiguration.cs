using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
    {
        public void Configure(EntityTypeBuilder<Quiz> builder)
        {
            builder.HasKey(q => q.Id);
            builder.Property(q => q.Title).IsRequired().HasMaxLength(200);
            builder.Property(q => q.Description).HasMaxLength(1000);

            // Quiz mütləq bir AssessmentItem-ə bağlıdır (1:1). AssessmentItem-də əks naviqasiya yoxdur.
            builder.HasOne(q => q.AssessmentItem)
                .WithOne()
                .HasForeignKey<Quiz>(q => q.AssessmentItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bir AssessmentItem üçün yalnız bir quiz (duplicate-in qarşısı). Soft-delete nəzərə alınır.
            builder.HasIndex(q => q.AssessmentItemId)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // GroupId yalnız sorğu üçün skalyar sütundur (Group-a formal FK yoxdur ki, cascade yolları toqquşmasın).
            builder.HasIndex(q => q.GroupId);

            builder.HasQueryFilter(q => !q.IsDeleted);
        }
    }
}
