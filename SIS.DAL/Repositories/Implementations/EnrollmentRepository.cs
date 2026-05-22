using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Enrollment>> GetAllAsync()
            => await _dbSet
                .Include(e => e.Student)
                    .ThenInclude(s => s.AppUser)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Teacher)
                        .ThenInclude(t => t.AppUser)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.AcademicTerm)
                .ToListAsync();

        public async Task<Enrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId)
            => await _dbSet
                .Include(e => e.Student)
                    .ThenInclude(s => s.AppUser)
                .Include(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
                .Include(e => e.Grades)
                .Include(e => e.AttendanceRecords)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        public async Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId)
            => await _dbSet
         .Include(e => e.Student)
             .ThenInclude(s => s.AppUser)
         .Include(e => e.CourseOffering)
             .ThenInclude(co => co.Course)
         .Include(e => e.CourseOffering)
             .ThenInclude(co => co.Teacher)
                 .ThenInclude(t => t.AppUser)
         .Include(e => e.CourseOffering)
             .ThenInclude(co => co.AcademicTerm)
         .Where(e => e.StudentId == studentId)
         .ToListAsync();

        public async Task<IEnumerable<Enrollment>> GetCourseEnrollmentsAsync(int courseOfferingId)
            => await _dbSet
                .Include(e => e.Student)
                    .ThenInclude(s => s.AppUser)
                .Where(e => e.CourseOfferingId == courseOfferingId)
                .ToListAsync();

        public async Task<bool> IsStudentEnrolledAsync(int studentId, int courseOfferingId)
            => await _dbSet.AnyAsync(e =>
                e.StudentId == studentId &&
                e.CourseOfferingId == courseOfferingId);
    }
}

