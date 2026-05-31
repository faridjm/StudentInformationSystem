using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class GroupRepository : GenericRepository<Group>, IGroupRepository
    {
        public GroupRepository(AppDbContext context) : base(context) { }

        public async Task<Group?> GetGroupWithDetailsAsync(int groupId)
            => await _dbSet
                .Include(g => g.Course)
                .Include(g => g.Teacher).ThenInclude(t => t.AppUser)
                .Include(g => g.AcademicYear)
                .Include(g => g.ScheduleDays)
                .Include(g => g.Enrollments).ThenInclude(e => e.Student).ThenInclude(s => s.AppUser)
                .FirstOrDefaultAsync(g => g.Id == groupId);

        public async Task<IEnumerable<Group>> GetByTeacherAsync(int teacherId)
            => await _dbSet
                .Include(g => g.Course)
                .Include(g => g.AcademicYear)
                .Include(g => g.ScheduleDays)
                .Include(g => g.Enrollments)
                .Where(g => g.TeacherId == teacherId)
                .ToListAsync();

        public async Task<IEnumerable<Group>> GetByCourseAsync(int courseId)
            => await _dbSet
                .Include(g => g.Teacher).ThenInclude(t => t.AppUser)
                .Include(g => g.AcademicYear)
                .Include(g => g.Enrollments)
                .Where(g => g.CourseId == courseId)
                .ToListAsync();

        public async Task<IEnumerable<Group>> GetActiveGroupsAsync()
            => await _dbSet
                .Include(g => g.Course)
                .Include(g => g.Teacher).ThenInclude(t => t.AppUser)
                .Include(g => g.AcademicYear)
                .Include(g => g.Enrollments)
                .Where(g => g.IsActive)
                .ToListAsync();
    }
}
