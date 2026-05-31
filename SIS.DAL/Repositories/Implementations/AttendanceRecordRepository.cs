using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Enums;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class AttendanceRecordRepository : GenericRepository<AttendanceRecord>, IAttendanceRecordRepository
    {
        public AttendanceRecordRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<AttendanceRecord>> GetAttendanceByEnrollmentAsync(int enrollmentId)
            => await _dbSet
                .Include(a => a.Enrollment).ThenInclude(e => e.Student).ThenInclude(s => s.AppUser)
                .Include(a => a.ClassSession)
                .Where(a => a.EnrollmentId == enrollmentId)
                .OrderBy(a => a.SessionDate)
                .ToListAsync();

        public async Task<IEnumerable<AttendanceRecord>> GetAttendanceBySessionAsync(int sessionId)
            => await _dbSet
                .Include(a => a.Enrollment).ThenInclude(e => e.Student).ThenInclude(s => s.AppUser)
                .Where(a => a.ClassSessionId == sessionId)
                .ToListAsync();

        public async Task<double> GetAttendancePercentageAsync(int enrollmentId)
        {
            var records = await _dbSet.Where(a => a.EnrollmentId == enrollmentId).ToListAsync();
            if (!records.Any()) return 0;
            var presentCount = records.Count(a => a.Status != AttendanceStatus.Absent);
            return Math.Round((double)presentCount / records.Count * 100, 1);
        }
    }
}
