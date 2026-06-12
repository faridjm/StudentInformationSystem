using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class ParentStudentConfiguration : IEntityTypeConfiguration<ParentStudent>
    {
        public void Configure(EntityTypeBuilder<ParentStudent> builder)
        {
            builder.HasKey(ps => ps.Id);

            builder.Property(ps => ps.ParentUserId).IsRequired().HasMaxLength(450);
            builder.Property(ps => ps.RelationshipType).HasConversion<int>();

            // Çoxlu cascade path-dan qaçmaq üçün Restrict (əlaqələri service idarə edir)
            builder.HasOne(ps => ps.ParentUser)
                .WithMany()
                .HasForeignKey(ps => ps.ParentUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ps => ps.Student)
                .WithMany()
                .HasForeignKey(ps => ps.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Eyni valideyn + tələbə cütü təkrarlanmasın
            builder.HasIndex(ps => new { ps.ParentUserId, ps.StudentId }).IsUnique();
            builder.HasIndex(ps => ps.ParentUserId);
            builder.HasIndex(ps => ps.StudentId);
            builder.HasIndex(ps => ps.IsActive);

            builder.HasQueryFilter(ps => !ps.IsDeleted);
        }
    }
}
