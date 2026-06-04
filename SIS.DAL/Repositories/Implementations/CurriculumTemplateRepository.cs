using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class CurriculumTemplateRepository : GenericRepository<CurriculumTemplate>, ICurriculumTemplateRepository
    {
        public CurriculumTemplateRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<CurriculumTemplate>> GetByCourseAsync(int courseId)
            => await _dbSet
                .Include(t => t.Items)
                .Where(t => t.CourseId == courseId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        public async Task<CurriculumTemplate?> GetActiveByCourseAsync(int courseId)
            => await _dbSet
                .Include(t => t.Items)
                .Where(t => t.CourseId == courseId && t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

        public async Task<CurriculumTemplate?> GetWithItemsAsync(int templateId)
            => await _dbSet
                .Include(t => t.Items)
                .FirstOrDefaultAsync(t => t.Id == templateId);
    }
}
