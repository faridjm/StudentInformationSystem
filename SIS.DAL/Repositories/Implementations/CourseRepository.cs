using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Course>> GetActiveCoursesAsync()
            => await _dbSet.Include(c => c.Groups).Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();

        public async Task<IEnumerable<Course>> GetAllWithGroupsAsync()
            => await _dbSet.Include(c => c.Groups).OrderBy(c => c.Name).ToListAsync();
    }
}
