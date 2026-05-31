using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
    {
        public void Configure(EntityTypeBuilder<ClassSession> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Topic).HasMaxLength(300);
            builder.Property(c => c.Notes).HasMaxLength(2000);

            builder.HasOne(c => c.Group)
                .WithMany(g => g.ClassSessions)
                .HasForeignKey(c => c.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
