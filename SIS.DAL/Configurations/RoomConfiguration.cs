using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
            builder.Property(r => r.Code).HasMaxLength(30);
            builder.Property(r => r.Location).HasMaxLength(150);
            builder.Property(r => r.Floor).HasMaxLength(50);
            builder.Property(r => r.Description).HasMaxLength(1000);
            builder.Property(r => r.Type).HasConversion<int>();

            // Otaq adı unikal (soft-delete olunmayanlar arasında query filter ilə)
            builder.HasIndex(r => r.Name).IsUnique();
            builder.HasIndex(r => r.IsActive);

            builder.HasQueryFilter(r => !r.IsDeleted);
        }
    }
}
