using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class WeeklySchedulePatternRepository
        : GenericRepository<WeeklySchedulePattern>, IWeeklySchedulePatternRepository
    {
        public WeeklySchedulePatternRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<WeeklySchedulePattern>> GetByCourseOfferingAsync(int courseOfferingId)
            => await _dbSet
                .Where(p => p.CourseOfferingId == courseOfferingId)
                .OrderBy(p => p.DayOfWeek)
                .ThenBy(p => p.StartTime)
                .ToListAsync();

        public async Task DeleteByCourseOfferingAsync(int courseOfferingId)
        {
            var patterns = await _dbSet
                .Where(p => p.CourseOfferingId == courseOfferingId)
                .ToListAsync();

            foreach (var p in patterns)
                SoftDelete(p);
        }
    }
}
