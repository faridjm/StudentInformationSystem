using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class ClassSessionRepository : GenericRepository<ClassSession>, IClassSessionRepository
    {
        public ClassSessionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<ClassSession>> GetByGroupAsync(int groupId)
            => await _dbSet
                .Where(s => s.GroupId == groupId)
                .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
                .ToListAsync();

        public async Task<IEnumerable<ClassSession>> GetByDateRangeAsync(int groupId, DateOnly from, DateOnly to)
            => await _dbSet
                .Where(s => s.GroupId == groupId && s.Date >= from && s.Date <= to)
                .OrderBy(s => s.Date)
                .ToListAsync();
    }
}
