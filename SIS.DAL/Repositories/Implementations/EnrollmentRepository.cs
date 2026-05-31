using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(AppDbContext context) : base(context) { }

        public async Task<Enrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId)
            => await _dbSet
                .Include(e => e.Student).ThenInclude(s => s.AppUser)
                .Include(e => e.Group).ThenInclude(g => g.Course)
                .Include(e => e.Group).ThenInclude(g => g.Teacher).ThenInclude(t => t.AppUser)
                .Include(e => e.Group).ThenInclude(g => g.AcademicYear)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        public async Task<IEnumerable<Enrollment>> GetByGroupAsync(int groupId)
            => await _dbSet
                .Include(e => e.Student).ThenInclude(s => s.AppUser)
                .Where(e => e.GroupId == groupId)
                .ToListAsync();

        public async Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId)
            => await _dbSet
                .Include(e => e.Group).ThenInclude(g => g.Course)
                .Include(e => e.Group).ThenInclude(g => g.Teacher).ThenInclude(t => t.AppUser)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();

        public async Task<bool> IsStudentEnrolledAsync(int studentId, int groupId)
            => await _dbSet.AnyAsync(e => e.StudentId == studentId && e.GroupId == groupId);
    }
}
