using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class AdminAccessGrantConfiguration : IEntityTypeConfiguration<AdminAccessGrant>
    {
        public void Configure(EntityTypeBuilder<AdminAccessGrant> builder)
        {
            builder.HasKey(g => g.Id);
            builder.Property(g => g.AdminUserId).IsRequired().HasMaxLength(450);
            builder.Property(g => g.GrantedBySuperAdminId).IsRequired().HasMaxLength(450);
            builder.Property(g => g.RevokedBySuperAdminId).HasMaxLength(450);
            builder.Property(g => g.Reason).HasMaxLength(500);

            // AppUser-ə formal FK qurulmur (naviqasiya yoxdur, cascade yolları sadə qalsın) —
            // yalnız sorğu üçün indeks.
            builder.HasIndex(g => g.AdminUserId);
            builder.HasIndex(g => g.IsActive);

            builder.HasQueryFilter(g => !g.IsDeleted);
        }
    }
}
