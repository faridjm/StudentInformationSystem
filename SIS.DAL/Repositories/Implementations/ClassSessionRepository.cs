using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class ClassSessionRepository : GenericRepository<ClassSession>, IClassSessionRepository
    {
        public ClassSessionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ClassSession?> GetWithDetailsAsync(int id)
            => await _dbSet
                .Include(s => s.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(s => s.CourseOffering)
                    .ThenInclude(co => co.Teacher)
                        .ThenInclude(t => t.AppUser)
                .Include(s => s.CourseOffering)
                    .ThenInclude(co => co.AcademicTerm)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<IEnumerable<ClassSession>> GetByCourseOfferingAsync(int courseOfferingId)
            => await _dbSet
                .Where(s => s.CourseOfferingId == courseOfferingId)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

        public async Task<IEnumerable<ClassSession>> GetByCourseOfferingAndDateRangeAsync(
            int courseOfferingId, DateOnly from, DateOnly to)
            => await _dbSet
                .Where(s => s.CourseOfferingId == courseOfferingId
                         && s.Date >= from
                         && s.Date <= to)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

        public async Task<IEnumerable<ClassSession>> GetByStudentAndDateRangeAsync(
            int studentId, DateOnly from, DateOnly to)
            => await _dbSet
                .Include(s => s.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(s => s.CourseOffering)
                    .ThenInclude(co => co.Teacher)
                        .ThenInclude(t => t.AppUser)
                .Where(s => s.CourseOffering.Enrollments
                                .Any(e => e.StudentId == studentId
                                       && e.Status == SIS.Entities.Enums.EnrollmentStatus.Active)
                         && s.Date >= from
                         && s.Date <= to)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

        public async Task<bool> ExistsAsync(int courseOfferingId, DateOnly date, TimeOnly startTime)
            => await _dbSet.AnyAsync(s =>
                s.CourseOfferingId == courseOfferingId
                && s.Date == date
                && s.StartTime == startTime);
    }
}
