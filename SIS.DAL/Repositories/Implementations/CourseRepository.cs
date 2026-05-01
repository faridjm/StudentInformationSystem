using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Course?> GetCourseWithDetailsAsync(int courseId)
            => await _dbSet
                .Include(c => c.Department)
                .Include(c => c.CourseOfferings)
                    .ThenInclude(co => co.Teacher)
                        .ThenInclude(t => t.AppUser)
                .FirstOrDefaultAsync(c => c.Id == courseId);

        public async Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(int departmentId)
            => await _dbSet
                .Where(c => c.DepartmentId == departmentId)
                .ToListAsync();

        public async Task<bool> IsCourseCodeUniqueAsync(string code)
            => !await _dbSet.AnyAsync(c => c.Code == code);
    }


}

