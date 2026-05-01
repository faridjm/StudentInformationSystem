using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class ActivityLogRepository : GenericRepository<ActivityLog>, IActivityLogRepository
    {
        public ActivityLogRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ActivityLog>> GetLogsByUserAsync(string userId)
            => await _dbSet
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<ActivityLog>> GetLogsByEntityAsync(string entityName)
            => await _dbSet
                .Include(a => a.User)
                .Where(a => a.EntityName == entityName)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<ActivityLog>> GetRecentLogsAsync(int count)
            => await _dbSet
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
    }
}

