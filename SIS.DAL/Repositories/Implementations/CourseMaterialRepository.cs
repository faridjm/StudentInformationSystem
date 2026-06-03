using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class CourseMaterialRepository : GenericRepository<CourseMaterial>, ICourseMaterialRepository
    {
        public CourseMaterialRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<CourseMaterial>> GetByGroupAsync(int groupId)
            => await _dbSet
                .Where(m => m.GroupId == groupId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<CourseMaterial>> GetVisibleByGroupAsync(int groupId)
            => await _dbSet
                .Where(m => m.GroupId == groupId && m.IsActive)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
    }
}
