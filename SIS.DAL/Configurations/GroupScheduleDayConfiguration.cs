using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Entities.Models;

namespace SIS.DAL.Configurations
{
    public class GroupScheduleDayConfiguration : IEntityTypeConfiguration<GroupScheduleDay>
    {
        public void Configure(EntityTypeBuilder<GroupScheduleDay> builder)
        {
            builder.HasKey(g => g.Id);

            builder.HasOne(g => g.Group)
                .WithMany(gr => gr.ScheduleDays)
                .HasForeignKey(g => g.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(g => !g.IsDeleted);
        }
    }
}
